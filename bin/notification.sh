notification() {
        # if command is notification based on command code app will understand what to do.
        # based on command code it will echo json message to mainBash
        # somehow need to execute this message again by mainBash
        # change mine command output with result

        fromSocket=$(echo ${jsonMessage}   | jq -r '.socketID'   |sed "s/\"//g")
        commandCode=$(echo ${jsonMessage}  | jq -r '.commandCode'|sed "s/\"//g")
        messageType=$(echo ${jsonMessage}  | jq -r '.messageType'|sed "s/\"//g")
        result=$(echo ${jsonMessage}  | jq -r '.result')
        command=$(mapFunction2Code ${commandCode} func)       
        # THIS MESSAGE SHOULD BE ROUTED TO LOCAL BASHCOIN.SH script. So, no need to define destinatioSockey ket in message.
        # Teorically this message cannot be routed itself. 
        msgTrimmed=$(echo "{\"command\":\"$command\",\"destinationSocket\": $fromSocket,\"from\":\"$(hostname)\",\"messageType\":\"direct\",\"result\":$result}" | tr '\n' ' ' | sed 's/ //g')
        echo $msgTrimmed
}
