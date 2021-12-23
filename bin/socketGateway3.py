#!/usr/bin/python
from websocket_server import WebsocketServer
import threading
import time
import json
import socket

import logging
import traceback
import requests

threads = []
clients = {}

#   {"command":"mine","appType":"wallet"} / {"command":"mine","appType":"miner","messageType":"direct"}
#   {"command":"notification","messageType":"direct"}
#   {"command":"checkbalance","ACCTNUM":"50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034","messageType":"direct"}
#   {"command":"listNewBlock","fromBlockID":70,"messageType":"direct"}
#   {"command":"provideBlocks","messageType":"direct","result": ["blk.pending","133.blk.solved","132.blk.solved","131.blk.solved"]}
#   {"command":"AddBlockFromNetwork","messageType":"direct","result": ["base64","base64"]}
#   {"command":"getTransactionMessageForSign","messageType":"direct","ACCTNUM":"50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034","RECEIVER":"b1bd54c941aef5e0096c46fd21d971b3a3cf5325226afb89c0a9d6845a491af6","AMOUNT":5,"FEE":3,"DATEEE":"202111121313"}
#   {"command":"pushSignedMessageToPending", "messageType": "direct", "result": ["50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:b1bd54c941aef5e0096c46fd21d971b3a3cf5325226afb89c0a9d6845a491af6:5:3:202111121313","50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:50416596951b715b7e8e658de7d9f751fb8b97ce4edf0891f269f64c8fa8e034:38:0:202111121313"]}
#   {"command":"validate"} // CLI


# This list for build connected device list,
#  client Graph algo will find whether it can reach main network or not.
localPublicIP = requests.get('https://checkip.amazonaws.com').text.strip()

def getRoot():
    result=''
    locals=[]
    for i in clients:
        if clients[i]['address'][0]=='127.0.0.1':
            locals.append(i)
    return locals

# Find local client which is binded core SH script (main)
def WhereBashCoin(jsonData,serchKey,valueIs,printKeyValue):
    ## This function return client ID of bashCoin.sh connection. It can be done by secret
    ## WhereBashCoin(clients,'destinationSocketBashCoin',<yes>,'id')
    for i in clients:
        if serchKey in jsonData[i]:
            if jsonData[i][serchKey]==str(valueIs):
                return jsonData[i][printKeyValue]

def client_left(client, server):
    nodeMessage={"command":"removeConnectionPeers","messageType":"direct","socketID":WhereBashCoin(clients,'destinationSocketBashCoin','yes','id'),"node":str(client['address'][0])}
    try:
        clients.pop(client['id'])
    except:
        print ("Error in removing client %s" % client['id'])
    #for cl in clients.values():
        #server.send_message(cl, str(nodeMessage))
    print (nodeMessage)
    server.send_message(clients[WhereBashCoin(clients,'destinationSocketBashCoin','yes','id')], str(nodeMessage).replace("u'","'").replace("'","\""))



#def new_client(client, server):
#    # connection list
#    clients[client['id']] = client
#    print ("Connected Example: "+str(clients))
#    server.send_message(clients[client['id']], str(msg).replace("u'","'").replace("'","\""))

def new_client(client, server):
    # connection list
    clients[client['id']] = client
    print ("Connected Example: "+str(clients))
    if client['address'][0] != '127.0.0.1':
        nodeMessage={"command":"addConnectionPeers","messageType":"direct","socketID":WhereBashCoin(clients,'destinationSocketBashCoin','yes','id'),"node1":localPublicIP,"node2":str(client['address'][0])}
        print (nodeMessage)
        #server.send_message(clients[client['id']], str(nodeMessage).replace("u'","'").replace("'","\""))
        server.send_message(clients[WhereBashCoin(clients,'destinationSocketBashCoin','yes','id')], str(nodeMessage).replace("u'","'").replace("'","\""))


def msg_received(client, server, msg):
    # Handle messages routing between clients
    global destination
    if msg != "":
        print ("-----START MESSAGE SCENARIO-----\nFrom: "+str(client['address']) +" incoming message: "+str(msg))
        try:
            msg=json.loads(str(msg).encode('utf-8'))
            msg.update({'socketID':client['id']})
            ## this is inital for communication_pipe client
            if 'destinationSocketBashCoin' in msg:
                client['destinationSocketBashCoin']=msg['destinationSocketBashCoin']
            if client['id'] in getRoot():
            ################################# MESAGE FROM LOCALHOST  #################################
                if 'destinationSocket' in msg:
                    # this message comes from bashCoin.sh. Becasue SH script sets destinationSocket based on SocketID.
                    #  and socketID setting by this script before sending to client[]
                    destination=msg['destinationSocket']
                    print ("-------\n 000 TO -> "+str(destination)+"\n"+"MSG -> "+str(msg))
                    cl = clients[destination]
                    server.send_message(cl, str(msg).replace("u'","'").replace("'","\""))
                else:
                    # Usually comes from local SH
                    exceptID=[] 
                    if msg['messageType']=='broadcast':
                        if 'exceptSocket' in msg:
                            exceptID.extend(msg['exceptSocket'])
                            exceptID.append(WhereBashCoin(clients,'destinationSocketBashCoin','yes','id'))
                            integer_map = map(int, exceptID)
                            exceptID = list(integer_map)
                            del integer_map
                        else:
                            exceptID.append(WhereBashCoin(clients,'destinationSocketBashCoin','yes','id'))
                        ## MAKE THIS BY SECRET FILE CODE
                        # python3 wsdump.py  -r --text '{"command":"nothing","appType":"nothing","destinationSocketBashCoin":"yes"}' ws://127.0.0.1:8001
                        for i in clients:
                            if clients[i]['id'] not in exceptID:
                                # message will not go to SH
                                #msg.update({'socketID':clients[i]['id']})
                                print ("-------\n 001 TO -> "+str(clients[i]['id'])+" and exceptID is "+str(exceptID)+"\n"+"MSG -> "+str(msg))
                                server.send_message(clients[i], str(msg).replace("u'","'").replace("'","\""))
                    else:
                        # messageType=direct and comes from Local (getRoot) means to SH
                        # put socketID to be able for get response by SH
                        destination=WhereBashCoin(clients,'destinationSocketBashCoin','yes','id')
                        msg.update({'socketID':client['id']})
                        # track message destination
                        print ("-------\n 002 TO -> "+str(destination)+"\n"+"MSG -> "+str(msg))
                        cl = clients[destination]
                        server.send_message(cl, str(msg).replace("u'","'").replace("'","\""))
            else:
            ################################### MESAGE FROM EXTERNAL ########################
                ## SECURITY: put command list from external to internal.
                if msg['command'] in ['help','AddNewBlockFromNode','provideBlockContent','notification','nothing','listNewBlock','getTransactionMessageForSign','checkbalance','pushSignedMessageToPending','provideTXMessage','AddTransactionFromNetwork','price']:
                    # socketID is message originator always
                    #msg.update({'socketID':client['id']})
                    if msg['messageType']=='direct':
                        ## MAKE THIS BY SECRET FILE CODE
                        print ("-------\n 003 TO -> "+str(WhereBashCoin(clients,'destinationSocketBashCoin','yes','id'))+"\n"+"MSG -> "+str(msg))
                        server.send_message(clients[WhereBashCoin(clients,'destinationSocketBashCoin','yes','id')], str(msg).replace("u'","'").replace("'","\""))
                    if msg['messageType']=='broadcast':
                        ## THIS IS DANGER. NEED TO CONTROL MESSAGE CONTENT not to Broadcast
                        for i in clients:
                            ## send to all except message originator. Even bashCoin.sh socket
                            if clients[i]['id'] != msg['socketID']:
                                # track message destination
                                print ("-------\n 004 TO -> "+str(clients[i]['id'])+"\n"+"MSG -> "+str(msg))
                                server.send_message(clients[i], str(msg).replace("u'","'").replace("'","\""))
        except Exception as e:
            logging.error(traceback.format_exc())
            print ("Problem "+str(msg)+", and PROBLEM is "+str(e))


server = WebsocketServer(host='0.0.0.0',port=8001,loglevel=logging.DEBUG)
#server = WebsocketServer(host='0.0.0.0',port=8001,key=certFile, cert="/root/peer2peer/cert/cert.pem",loglevel=logging.DEBUG)
server.set_fn_client_left(client_left)
server.set_fn_new_client(new_client)
server.set_fn_message_received(msg_received)
server.run_forever()
