validateTransactionMessage() {
        ## ADD EACH TRANSCATION ACCOUNT CHECK (historical )
        txMessage=$1
        randomFolder=$RANDOM
        tempFolder="$tempRootFolder/$randomFolder"
        mkdir $tempFolder
        echo ${txMessage}| awk -v FS=':' '{print $1":"$2":"$3":"$4":"$5":"$6}' > $tempFolder/${randomFolder}_transaction.msg
        echo ${txMessage}| awk -v FS=':' '{print $7}'| base64 -d > $tempFolder/${randomFolder}_transaction.pub
        echo ${txMessage}| awk -v FS=':' '{print $8}'| base64 -d > $tempFolder/${randomFolder}_transaction.sig
        openssl dgst -verify $tempFolder/${randomFolder}_transaction.pub -keyform PEM -sha256 -signature $tempFolder/${randomFolder}_transaction.sig -binary $tempFolder/${randomFolder}_transaction.msg > /dev/null
}


getTransactionMessageForSign() {
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]} )
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        #fromSocket=$(echo ${jsonMessage}  | jq -r '.getTransactionMessageForSign')
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID'|sed "s/\"//g")
        SENDER=$(echo ${jsonMessage}  | jq -r '.ACCTNUM'|sed "s/\"//g")
        RECEIVER=$(echo ${jsonMessage}  | jq -r '.RECEIVER'|sed "s/\"//g")
        AMOUNT=$(echo ${jsonMessage}  | jq -r '.AMOUNT'|sed "s/\"//g")
        FEE=$(echo ${jsonMessage}  | jq -r '.FEE'|sed "s/\"//g")
        DATEEE=$(echo ${jsonMessage}  | jq -r '.DATEEE'|sed "s/\"//g")

        [[ -z $FEE ]] && echo "ERROR: You need to set FEE for miners validation." && exit
        [ $AMOUNT -le $FEE ] && echo "ERROR: Fee alwasys should be less than AMOUNT" && exit

        ## Fariz patch 
        [ $AMOUNT -le 0 ] && echo "Amount cannot be negative or zero" && exit 

        # Check if current block is solved just to make sure the blockchain is still live
        CURRENTBLOCKCHECK=`ls -1 *.blk $BLOCKPATH | tail -1 | grep solved`
        if [[ $CURRENTBLOCKCHECK ]]
                        then            
                                        ## change this message to Json
                                        echo "The current block is already solved!"
                                        echo "ERROR: need unsolved block"
                                        exit 1
        fi

        #TOTAL=$(checkAccountBal $SENDER 1| awk -v FS=':' '{print $2}')
        TOTAL=$(checkAccountBal \'{"command":"checkbalance","ACCTNUM":"$SENDER"} \' |  jq '.result'  | jq '.balance'| sed "s/\"//g")

        [[ $AMOUNT -gt $TOTAL ]] &&
        echo "{ \"command\":\"getTransactionMessageForSign\",\"messageType\":\"direct\",\"commandCode\":\"$errorCode\",\"status\":2,\"destinationSocket\":$fromSocket,\"SENDER\":\"$ACCTNUM\",\"message\":\"Insufficient Funds!\"}" && exit 1
        
        dateTime=$DATEEE

        #echo "Success! Sent $AMOUNT bCN to $RECEIVER and fee is: $FEE"

        ## ADDING BY SYSTEM 
        CHANGE=`echo $TOTAL-$AMOUNT-$FEE | bc`
        echo "{ \"command\":\"getTransactionMessageForSign\",\"messageType\":\"direct\", \"status\":0,\"destinationSocket\":$fromSocket,\"result\":{\"forReciverData\":\"$SENDER:$RECEIVER:$AMOUNT:$FEE:$dateTime\",\"forSenderData\":\"$SENDER:$SENDER:$CHANGE:0:$dateTime\"}}"
}

pushSignedMessageToPending() {
    ##########################################################################################
    ## from WALLET                                                                          ##
    ## 1. need to get message                                                               ##
    ## 2. add TX:Hash sha256 (message)                                                      ##
    ## 3. Sign and send back                                                                ##
    ##########################################################################################
    commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
    errorCode=$(mapFunction2Code ${FUNCNAME[0]})
    fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
    forReciverData=$(echo ${jsonMessage} | jq -r '.result' | jq -r '.[0]')
    forSenderData=$( echo ${jsonMessage} | jq -r '.result' | jq -r '.[1]')
    ## here we can validate it first before pushing to Pending Transaction
    txIDReciever=$(echo ${forReciverData}| awk -v FS=':' '{print $1}')
    txIDSender=$(  echo ${forSenderData} | awk -v FS=':' '{print $1}')
    recordExist=$(cat $BLOCKPATH/blk.pending | grep  "$txIDReciever\|$txIDSender")
    if [ ${#recordExist} -le 5 ]; then 
        validateTransactionMessage $forReciverData || exit 1
        validateTransactionMessage $forSenderData  || exit 1
        echo "$forReciverData" >> $BLOCKPATH/blk.pending  && echo "$forSenderData"  >> $BLOCKPATH/blk.pending
        if [ $? -eq 0 ]; then
                echo "{\"command\":\"notification\",\"status\":0,\"commandCode\":\"401\",\"status\":0,\"messageType\":\"broadcast\",\"result\":[\"$txIDReciever\",\"$txIDSender\"]}"
        fi
     else
        echo "{\"command\":\"pushSignedMessageToPending\",\"commandCode\":\"$commandCode\",\"status\":\"$errorCode\",\"messageType\":\"direct\",\"destinationSocket\":$fromSocket,\"commandCode\":\"$commandCode\"}"      
     fi
}

provideTXMessage() {
        ## THIS IS 401 which trigger by first notification chain
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        errorCode=$(mapFunction2Code ${FUNCNAME[0]})
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        txList=$(echo ${jsonMessage} | jq -r '.result')
        fullTXmessages=[]
        for TX in $(echo ${txList}| jq -c .[]); do
                TX=$(echo ${TX}| sed "s/\"//g")
                for blockFilesAndPending in $(ls -1 ${BLOCKPATH}/*blk* | sort -r); do
                        transaction=$(cat ${blockFilesAndPending}| grep $TX )
                        if [ $? -eq 0 ] ; then
                                fullTXmessages=$(echo $fullTXmessages| jq --arg dt $transaction '. + [ $dt ]')
                                break
                        fi
                done
        done
        if [ $(echo $fullTXmessages| jq length) == 2 ]; then
                #echo $fullTXmessages| jq length > $tempRootFolder/count_tx
                msg=$(echo "{\"command\":\"AddTransactionFromNetwork\",\"messageType\":\"direct\",\"destinationSocket\": $fromSocket,\"status\":0,\"commandCode\":\"$commandCode\",\"result\":$fullTXmessages }"|tr '\n' ' ' | sed 's/ //g'  )
                echo $msg
        else
                msg=$(echo "{\"command\":\"AddTransactionFromNetwork\",\"messageType\":\"direct\",\"destinationSocket\": $fromSocket,\"status\":$errorCode,\"commandCode\":\"$commandCode\",\"result\":$fullTXmessages }"|tr '\n' ' ' | sed 's/ //g'  )        
                echo $msg
        fi
}

AddTransactionFromNetwork() {
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        txIDlist=[]
        exceptSocket=[]
        for transactions in $(echo ${jsonMessage}| jq -r '.result'| jq -c .[]); do
                transaction=$(echo ${transactions}| sed "s/\"//g")
                txID=$(echo ${transaction}| awk -v FS=':' '{print $1}'| sed "s/\"//g")
                ls -1 $BLOCKPATH/*blk* | sort -r | xargs grep $txID > /dev/null
                [ $? -eq 0 ] && exit 1
                echo $transaction >> $BLOCKPATH/blk.pending
                txIDlist=$(echo ${txIDlist}| jq --arg dt $txID '. + [ $dt ]')
        done
        echo $txIDlist > $tempRootFolder/txIDlist
        exceptSocket=$(echo ${exceptSocket} | jq --arg dt $fromSocket '. + [ $dt ]'|sed "s/\"//g" )
        msg=$(echo "{\"command\":\"notification\",\"tag\":\"FFFFx0\",\"status\":0,\"commandCode\":\"$commandCode\",\"status\":0,\"exceptSocket\":$exceptSocket,\"messageType\":\"broadcast\",\"result\":$txIDlist}"| tr '\n' ' ' | sed 's/ //g')
        echo $msg
}


