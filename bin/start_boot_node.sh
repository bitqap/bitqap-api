#!/bin/bash

# INTERNAL COMMUNICATION 
# this script still will connect to LOCALHOST WS

# config include
. ../config/config.ini

## INITIALIZE
rm -rf $NAMED_PIPE_BSH
mkfifo $NAMED_PIPE_BSH
[ ! -p $NAMED_PIPE_BSH ] && mkfifo $NAMED_PIPE_BSH

# create independend strem (FILE DESCRIPTOR)
exec 3<> $NAMED_PIPE_BSH

main() {
 ## script will output to local named pipe and additional to external named pipe to send/recv info
 cat $NAMED_PIPE_BSH - | python3 wsdump.py  -r --text '{"command":"nothing","appType":"nothing","destinationSocketBashCoin":"yes","messageType":"direct"}' ws://127.0.0.1:8001 | while read line; do   
   echo ${line}| jq . > /dev/null 
   if [ $? -eq 0 ]; then     
     line=$(echo -e ${line});
     cmd="./$MAINSHELL '${line}' > $NAMED_PIPE_BSH"; 
     eval $cmd;
     echo "$(date -u)|info|$cmd"  >> $INTERNAL_COMM_LOG
   else
     echo "$(date -u)|error|$cmd" >> $INTERNAL_COMM_LOG
   fi; 
 done
}

main
