#!/bin/bash

# EXTERNAL COMMUNICATION 
# this script still will connect to predefined external neighbor IP.

# config include
. ../config/config.ini

# INITIALIZE
[ ! -p $NAMED_PIPE_EXT ] && mkfifo $NAMED_PIPE_EXT || exec 3<> $NAMED_PIPE_EXT

# Discover nodes
# declare -a predefinedPeer=("161.97.69.136")

# my IP:
myIP=$(curl https://checkip.amazonaws.com)

discoverNode() {
   # this function will discover all connections from boot node (predefined)
   # day by day boot nodes will be updated once trusted
   
   discoverFromBoot=$(python3 wsdump.py --eof-wait 2 -r -t '{"command":"peerInfo","messageType":"direct"}' ws://backbonix.com:8001 &)
   wait
   # adding patch. very bad solution.                                     \
   # when connection happend there are to JSON coming continusly.         \
   # Since, I could not distinguish connection type (miner, wallet, dump) \
   # decided to parse '}' and work with second part.                      \
   discoverFromBoot=$(echo ${discoverFromBoot}| awk -v FS='}' '{print $2"}]}"}')
   
   wLIST=[]               # weights
   connIDList=[]          # ip lists

   # fill above lists by discovery
   for i in $(echo $discoverFromBoot| jq -r '.result'|jq -c .[]); 
   do
      weight=$(echo $i| jq -r '.weight')
      ip=$(echo $i| jq -r '.peer')
      wLIST=$(echo ${wLIST}| jq --arg dt $weight '. + [ $dt ]' )
      connIDList=$(echo ${connIDList}| jq --arg dt $ip '. + [ $dt ]' )
   done

   wLIST=$(echo ${wLIST} | jq --sort-keys -c )
   export midINX=$(echo "scale=0 ; $(echo $wLIST | jq length ) / 2" | bc)
   export connID=$(echo $connIDList | jq .[$midINX] | sed "s/\"//g")
   export connIDList
   echo $connID
}

reDiscoverNode () {
   # this function will use rediscover if connection is fail.
   # rediscover will use -1 index, means retry more trusted connection (who maintain more connection)
   unsuccessConnectionIndex=$1
   let " retryConnectionID = $unsuccessConnectionIndex - 1 "
   [ $retryConnectionID -ge 0 ] && connID=$(echo $connIDList | jq .[$retryConnectionID] | sed "s/\"//g") || connID=$connID
   echo $connID
}



testConnection() {
   # if JSON output can be parsed  meand connected.
   PeerIP=$1
   connectionTest=$(python3 wsdump.py --eof-wait 2 -r -t '{"command":"peerInfo","messageType":"direct","myip":"$myIP"}' ws://$connID:8001 & )
   connectionTest=$(echo ${connectionTest}| awk -v FS='}' '{print $2"}]"}')
   wait
   parseTest=$(echo ${connectionTest}| jq > /dev/null)
   [ $? -eq 0 ] && echo 1 || echo 0
}


main() {
   # discover node
   discoverNode
   # test discovered node
   while [ $(testConnection ${connID} ) -eq 1 ]; do   # retry until test is successfull
      reDiscoverNode
   done

   cat $NAMED_PIPE_EXT - | python3 wsdump.py  -r  ws://$connID:8001 | while read line; do   
   echo ${line}| jq . > /dev/null;  
   if [ $? -eq 0 ]; then     
      line=$(echo -e ${line});
      cmd="./$MAINSHELL  '${line}' ";
      result=$(eval $cmd);
      tag=$(echo ${result}| jq -r '.tag')
      # if tag is FFFFx0 then push to Local named pipe to broadcast it.  
      [ "$tag" == "FFFFx0" ] && echo $result > $NAMED_PIPE_BSH || echo $result > $NAMED_PIPE_EXT
      # LOGGING
      echo "$(date -u)|info|$line" >> $EXTERNAL_COMM_LOG
   else
      # ERROR LOGGING
      echo "$(date -u)|error|$line" >> $EXTERNAL_COMM_LOG
   fi; 
 done
}

main