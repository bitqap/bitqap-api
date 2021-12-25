
listNewBlock() {
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        fromBlockID=$(echo ${jsonMessage}  | jq -r '.fromBlockID')
        lastBlockInLocal=$(ls -1 $BLOCKPATH| grep "solved$\|blk$"| sort -n -k 1 | tail -n 1| awk -v FS='.' '{print $1}')
        listMissingBlocksID=$(echo $lastBlockInLocal-$fromBlockID | bc)
        if [ $listMissingBlocksID -ge 0 ]; then
                listMissingBlocks=$(ls -1 $BLOCKPATH | grep "solved$\|blk$"| sort -nr -k 1 | head -$listMissingBlocksID)
                echo "{\"command\":\"listNewBlock\",\"commandCode\":\"$commandCode\",\"destinationSocket\":$fromSocket,\"status\":\"0\",\"result\":{\"blockList\": [\""$(echo ${listMissingBlocks}| awk 'BEGIN { OFS = "\",\"" } { $1 = $1; print }')\""]}}"
        fi
}


askBlockContent() {
    # this message will send commandCode=301 to execute ProvideBlockContent function in remote
    commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
    errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
    fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
    fileList=[]
    for blockInfo in  $(echo $jsonMessage | jq -r '.result'|jq -c .[] ); do
        fileID=$(echo ${blockInfo}| jq -r '.ID')
        hash=$(echo ${blockInfo}| jq -r '.hash')   #not use yet
        # check values are not empty. Otherwise exit 1
        [ ${#fileID} -ne 0 ] && [ ${#hash} -ne 0 ] || exit 1
        fileList=$(echo ${fileList} | jq --arg dt $fileID '. + [ $dt ]') 
        # check file not exist. Otherwise ignore it with exit 1
        #[ ! -f ${BLOCKPATH}/$fileID ] && fileList=$(echo $fileList | jq --arg dt $fileID '. + [ $dt ]') || exit 1
    done
    # if below code not exit, then send 301 as notification to destination to execute ProvideBlockContent.
    echo "{\"command\":\"provideBlockContent\",\"commandCode\":\"$commandCode\",\"messageType\":\"direct\",\"result\":$fileList}"
}


validateNetworkBlockHash() {
        folder=$1
        curr=$(pwd)
        flagByPassFolder=1   # only in first attempt.
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        cd $folder
        # Check that there are blocks to validate
        getLastBlockToAdd=$(ls -1 $BLOCKPATH| grep solved$ |sort -n| tail -n 1)
        for i in `ls -1 *solved| sort -n| tail -n +1`; do
                        if [ "$flagByPassFolder" == "1" ]; then
                            h=$BLOCKPATH/$getLastBlockToAdd
                            flagByPassFolder=0
                        else
                            h=`ls -1 | grep solved$ | sort -n| grep -B 1 $i | head -1`
                        fi
                        [ ${#h} -le 2 ] && exit 1     # it means, next file name is not exist. Probably .blk file (next ID)
                        PREVHASH=`md5sum $h | cut -d" " -f1`
                        j=`ls -1 | egrep "solved$|blk$" | sort -n| grep -A 1 $i | head -2| grep -v $i | tail -1`
                        CALCHASH=`sed "2c$PREVHASH" $i | md5sum | cut -d" " -f1`
                        REPORTEDHASH=`sed -n '2p' $j`
                        [[ $CALCHASH != $REPORTEDHASH ]] && 
                            echo "{\"command\": \"validateNetworkBlockHash\",\"commandCode\":\"$errorCode\",\"messageType\":\"direct\",\"destinationSocket\": $fromSocket,\"status\": \"2\",\"message\":\"FFFFx0 Hash mismatch!  $i has changed!  Do not trust any block after and including $i\"}" && 
                            exit 1
        done
        cd $curr
}

checkBlockSignature() {
    blockFile=$1
    baseBlockName=$(echo $blockFile| rev | awk -v FS='/' '{print $1}'| rev)
    tempD=$tempRootFolder/$RANDOM
    mkdir $tempD
    cat $blockFile | grep BlockSignPublicKey | awk -v FS=':' '{print $2}'|sed "s/ //"|base64 -d > $tempD/$baseBlockName.pub
    cat $blockFile | grep BlockSignSIGNATURE | awk -v FS=':' '{print $2}'|sed "s/ //"|base64 -d > $tempD/$baseBlockName.sign 
    sed -e '/BlockSignSIGNATURE/,$d' $blockFile > $tempD/$baseBlockName.raw 
    openssl dgst -verify $tempD/$baseBlockName.pub -keyform PEM -sha256 -signature $tempD/$baseBlockName.sign  -binary $tempD/$baseBlockName.raw  > /dev/null
}



provideBlockContent() {
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        ### IF BLOCK NOT EXIST THEN ASK. OTHERWISE NOTHING TO DO
        blockMessage=[]
        fileIDs=$(echo $jsonMessage  | jq -r '.result'| tr '\n' ' ' | sed 's/ //g')
        for i in $(echo $jsonMessage  | jq -r '.result'| jq -c '.[]'| sed "s/\"//g"); do
                blockBase64=$(cat ${BLOCKPATH}/$i | base64 | tr '\n' ' ' | sed 's/ //g')
                blockMessage=$(echo ${blockMessage}| jq --arg dt $blockBase64 '. + [ $dt ]')
                #blockMessage=$(echo "$blockMessage\"$blockBase64\",")
        done
        # DONT SEND destination. By default it will route to session itself (bashCoin.sh) se
        msg=$(echo "{\"command\": \"AddNewBlockFromNode\",\"host\":\"$(pwd)\",\"tag\":\"FFFFx1\",\"commandCode\":\"$commandCode\",\"destinationSocket\": $fromSocket,\"messageType\":\"direct\",\"status\": \"0\",\"fileIDs\":$fileIDs,\"result\":$blockMessage}"|tr '\n' ' ' | sed 's/ //g')
        echo $msg
        # Get list and parse JSON
        # if 30% then provide message to download full copy (optional)
}


removeTransactionsFromPending() {
    files=$1
    cat ${files[@]} | grep ^TX | while read tx; do
        sed -i "/$tx/d" $BLOCKPATH/blk.pending
    done
}


AddNewBlockFromNode() {
    commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
    errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
    exceptSocket=[]
    fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
    results=$(echo $jsonMessage | jq -r '.result')
    fileIDs=$(echo $jsonMessage  | jq -r '.fileIDs'| tr '\n' ' ' | sed 's/ //g')
    blocksTemp=$tempRootFolder/block_$RANDOM
    mkdir -p $blocksTemp
    count=0
    ERRORS=0
    for newBloks in  $(echo $jsonMessage | jq -r '.result' | jq -c .[]); do
        let " count = $count + 1 "
        echo $newBloks |sed "s/\"//g"| base64 -d > $blocksTemp/$count.blk 
        BlockID=$(cat $blocksTemp/$count.blk | grep BLOCKID | awk -v FS=':' '{ print $2}'| sed "s/ //g")

        if [ ${#BlockID} -ge 4 ] ; then
            mv $blocksTemp/$count.blk $blocksTemp/$BlockID
        fi
        
        if [ -f ${blocksTemp}/${count}.blk ] && [ $(cat ${blocksTemp}/${count}.blk | wc -l) -eq 3 ] && [[ $(cat ${blocksTemp}/${count}.blk | grep "Previous Block Hash") ]]; then
            continue
        fi

        if [ $(echo ${BlockID}| awk -v FS='.' '{print $3}') == "solved" ]; then
            checkBlockSignature $blocksTemp/$BlockID
        else
            echo "{\"command\":\"AddNewBlockFromNode\",\"destinationSocket\": $fromSocket,\"messageType\":\"direct\",\"commandCode\":\"$errorCode\",\"status\":"2",\"message\":\"Cheating in chain, block sign issue\"}"
            exit 1
        fi

        cat $blocksTemp/$BlockID |  grep '^TX' |while read line; do
            # STARTING NEW LOOP to read all TX
            validateTransactionMessage $line
            if [ $? -ne 0 ]; then
                echo "{\"command\":\"$command\",\"commandCode\":\"$commandCode\",\"status\":$errorCode,\"message\":\"Some transactions cannot be validated. it is destroy whole signature of block\"}" 
                exit 1
            fi
            done
    done
    
    expcetedNtwrkBlockID=$(ls -1 ${BLOCKPATH}  | sort -n | grep "\.solved$" | sort -n |tail -n 1| awk -v FS='.blk.solved' '{print $1+1}')
    comingNtwrkBlockID=$(ls -1 ${blocksTemp}   | sort -n | grep "\.solved$" | sort -n |head -1  | awk -v FS='.blk.solved' '{print $1}')
    
    ## change blk file to current+1 for validataion chain and expire old one.
    [ $(ls ${blocksTemp}/*.blk | wc -l) -eq 1 ] && mv ${blocksTemp}/*.blk ${blocksTemp}/$(expr ${comingNtwrkBlockID} + 1).blk || exit 1

    if [ "$expcetedNtwrkBlockID" == "$comingNtwrkBlockID" ];then
        validateNetworkBlockHash "$blocksTemp"
        ret=$?
            if [ $ret -eq 0 ]; then
                blocks=$(ls ${blocksTemp}/*blk.solved*)
                removeTransactionsFromPending $blocks
                # expire old blk and move all to BLOCKCHAIN
                ls $BLOCKPATH/*.blk | sort -n|tail -n 1 | xargs -I {} mv {} {}.expired && mv $blocksTemp/*blk* $BLOCKPATH/
                # broadcast this message. But not to same session (somehow) and local root.
                exceptSocket=$(echo ${exceptSocket} | jq --arg dt $fromSocket '. + [ $dt ]' |sed 's/"//g')
                # THIS MESSAGE FOR ROUTE local PIPE to broadcast (remove new lines)
                msg=$(echo "{\"command\":\"notification\",\"tag\":\"FFFFx0\",\"status\":0,\"commandCode\":\"301\",\"messageType\":\"broadcast\",\"exceptSocket\":$exceptSocket,\"result\":$fileIDs}" |tr '\n' ' ' | sed 's/ //g' )
                echo $msg
            fi
    else
            echo "{\"command\":\"AddNewBlockFromNode\",\"host\":\"$(pwd)\",\"commandCode\":\"$commandCode\",\"messageType\":\"direct\",\"destinationSocket\": $fromSocket,\"status\":$errorCode,\"message\":\"Function: validateNetworkBlockHash , Folder:$blocksTemp, firstFileNetwID=$firstFileNetwID and lastFileCurrIDplus1=$lastFileCurrIDplus1 Chain ID $BlockID is not matching\"}"
            exit 1
    fi
}
