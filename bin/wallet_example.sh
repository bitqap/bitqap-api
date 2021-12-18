

#full message
fullMessage='{"command": "getTransactionMessageForSign", "status": 0, "destinationSocket": "2", "result": {"forReciverData": "50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:b1bd54c941aef5e0096c46fd21d971b3a3cf5325226afb89c0a9d6845a491af6:5:3:202111121313", "forSenderData": "50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:38:0:202111121313"}}'

# key pair
privKey="/root/bashCo1/cert/example.com.key"
publKey="/root/bashCo1/cert/example.com.pub"

# orignal raw message need to be hashed for TxID
rawMsgRecv=$(echo $fullMessage | jq -r '.result' | jq -r '.forReciverData')
rawMsgSend=$(echo $fullMessage | jq -r '.result' | jq -r '.forSenderData')

# hash sha256
txIDRecv=$(echo ${rawMsgRecv}| sha256sum | awk '{print $1}')
txIDSend=$(echo ${rawMsgSend}| sha256sum | awk '{print $1}')

# raw message need to be signed (txID included)
OrigMSG_Recv=$(echo "TX$txIDRecv:$rawMsgRecv")
OrigMSG_Send=$(echo "TX$txIDSend:$rawMsgRecv")

# encoded Public key as base64 
pubKeyBase64=$(cat ${publKey}| base64 | tr '\n' ' ' | sed 's/ //g')

# signatures of both messages
signedMsgRecv=$(echo ${OrigMSG_Recv}| openssl dgst -sign ${privKey} -keyform PEM -sha256 | base64 | tr '\n' ' ' | sed 's/ //g')
signedMsgSend=$(echo ${OrigMSG_Send}| openssl dgst -sign ${privKey} -keyform PEM -sha256 | base64 | tr '\n' ' ' | sed 's/ //g')

# construct of messages
recv="$OrigMSG_Recv:$pubKeyBase64:$signedMsgRecv"
send="$OrigMSG_Send:$pubKeyBase64:$signedMsgSend"

# build JSON
result=[]
# add reciever msg
result=$(echo ${result}| jq --arg dt $recv '. + [ $dt ]')
# add sender msg
result=$(echo ${result}| jq --arg dt $send '. + [ $dt ]')

# Full messages
returnMesssage="{\"command\":\"pushSignedMessageToPending\", \"messageType\": \"direct\", \"result\":$result}"


echo $returnMesssage



