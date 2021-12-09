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

        ########### SAMPLES ###########
        #       {"command":"provideBlocks","messageType":"direct","result": ["blk.pending","133.blk.solved","132.blk.solved","131.blk.solved"]}
        #       {"command":"AddBlockFromNetwork","messageType":"direct","result": ["base64","base64"]}
        #       {"command":"pushSignedMessageToPending", "messageType": "direct", "result": ["50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:b1bd54c941aef5e0096c46fd21d971b3a3cf5325226afb89c0a9d6845a491af6:5:3:202111121313", "50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:38:0:202111121313"]}
        
        # THIS MESSAGE SHOULD BE ROUTED TO LOCAL BASHCOIN.SH script. So, no need to define destinatioSockey ket in message.
        # Teorically this message cannot be routed itself. 
        msgTrimmed=$(echo "{\"command\":\"$command\",\"commandKodu\":$commandCode,\"addionalinfo\":\"$(pwd)\",\"messageType\":\"direct\",\"result\":$result}" | tr '\n' ' ' | sed 's/ //g')
        echo $msgTrimmed
}