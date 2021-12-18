#!/usr/bin/python3

import OpenSSL
from OpenSSL import crypto
import base64
import sys
import json
import hashlib

fullMessage='{"command": "getTransactionMessageForSign", "status": 0, "destinationSocket": "2", "result": {"forReciverData": "50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:b1bd54c941aef5e0096c46fd21d971b3a3cf5325226afb89c0a9d6845a491af6:5:3:202111121313", "forSenderData": "50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:38:0:202111121313"}}'
fullMessage=json.loads(fullMessage)
#SIGN=$(echo TX$HASH_TRANSACTION:REWARD:${SENDER}:${REWARD_COIN}:0:${dateTime} | openssl dgst -sign ${privateKeyFile} -keyform PEM -sha256 -passin pass:$Password| base64 | tr '\n' ' ' | sed 's/ //g')


#fullMessage=json.loads(sys.argv[1])

pubFile="/root/bashCo1/cert/example.com.pub"
key_file="/root/bashCo1/cert/example.com.key"

def Achar(privKey=key_file,publikKey=pubFile):
    pubFile=open('/root/bashCo1/cert/example.com.pub').read()
    key_file = open("/root/bashCo1/cert/example.com.key", "r")
    key = key_file.read()
    key_file.close()
    if key.startswith('-----BEGIN '):
        pkey = crypto.load_privatekey(crypto.FILETYPE_PEM, key)
    else:
        pkey = crypto.load_pkcs12(key).get_privatekey()
    return pkey

pkey=Achar()


def signMessage(txMessage,privKey=None):
    txHash = hashlib.sha256(txMessage.encode('utf-8')).hexdigest()
    data="TX"+str(txHash)+":"+str(txMessage)
    sign = OpenSSL.crypto.sign(pkey, data.encode(), "sha256")
    data_base64 = base64.b64encode(sign)
    pub=base64.b64encode(pubFile.encode())
    result=data+":"+str(pub)+":"+str(data_base64.decode())
    print (result)


print ("---- for reciever-----")
signMessage(fullMessage['result']['forReciverData']+"\n")
print ("---- for sender-----")
signMessage(fullMessage['result']['forSenderData']+"\n")

## {"command":"pushSignedMessageToPending", "messageType": "direct", "result": [""]}


