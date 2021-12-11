#!/bin/bash

# EXTERNAL COMMUNICATION 
# this script still will connect to predefined external neighbor IP.

# config include
. ../config/config.ini

## INITIALIZE
[ ! -p $NAMED_PIPE_BSH ] && mkfifo $NAMED_PIPE_BSH || exec 3<> $NAMED_PIPE_BSH
[ ! -p $NAMED_PIPE_EXT ] && mkfifo $NAMED_PIPE_EXT || exec 3<> $NAMED_PIPE_EXT


main() {
 cat $NAMED_PIPE_EXT - | python3 wsdump.py  -r  ws://backbonix.com:8001 | while read line; do   
   echo ${line}| jq . > /dev/null;  
   if [ $? -eq 0 ]; then     
      line=$(echo -e ${line});
      appID=$(echo ${line}| jq -r '.appID')
      [ "$appID" != "externalApp" ] && line=$(echo ${line}| jq '.appID="externalApp"')
      cmd="./$MAINSHELL  '${line}' > $NAMED_PIPE_EXT";
      # here will be validation message function.
      eval $cmd;
      echo "$(date -u)|info|$cmd" >> $EXTERNAL_COMM_LOG
   else
      echo "$(date -u)|error|$cmd" >> $EXTERNAL_COMM_LOG
   fi; 
 done
}

main

