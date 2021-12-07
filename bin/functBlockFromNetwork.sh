
listNewBlock() {
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        fromBlockID=$(echo ${jsonMessage}  | jq -r '.fromBlockID')
        lastBlockInLocal=$(ls -1 $BLOCKPATH| grep "solved$\|blk$"| sort -n -k 1 | tail -n 1| awk -v FS='.' '{print $1}')
        listMissingBlocksID=$(echo $lastBlockInLocal-$fromBlockID | bc)
        if [ $listMissingBlocksID -ge 0 ]; then
                listMissingBlocks=$(ls -1 $BLOCKPATH | grep "solved$\|blk$"| sort -nr -k 1 | head -$listMissingBlocksID)
                #echo "{\"command\":\"listNewBlock\",\"destinationSocket\":\"$fromSocket\",\"status\":\"0\",\"result\":{\"blockList\": [\"blk.pending\",\""$(echo ${listMissingBlocks}| awk 'BEGIN { OFS = "\",\"" } { $1 = $1; print }')\""]}}"
                echo "{\"command\":\"listNewBlock\",\"commandCode\":\"$commandCode\",\"destinationSocket\":\"$fromSocket\",\"status\":\"0\",\"result\":{\"blockList\": [\""$(echo ${listMissingBlocks}| awk 'BEGIN { OFS = "\",\"" } { $1 = $1; print }')\""]}}"
        fi
}


validateNetworkBlockHash() {
        folder=$1
        curr=$(pwd)
        #[ -z $flagByPassFolder ] && flagByPassFolder=1 || flagByPassFolder=0
        # only in first attempt
        flagByPassFolder=1
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
                        PREVHASH=`md5sum $h | cut -d" " -f1`
                        j=`ls -1 | egrep "solved$|blk$" | sort -n| grep -A 1 $i | head -2| grep -v $i | tail -1`
                        CALCHASH=`sed "2c$PREVHASH" $i | md5sum | cut -d" " -f1`
                        REPORTEDHASH=`sed -n '2p' $j`
                        [[ $CALCHASH != $REPORTEDHASH ]] &&  
                        echo "{\"command\": \"validateNetworkBlockHash\",\"commandCode\":\"$errorCode\",\"messageType\":\"direct\",\"destinationSocket\": \"$fromSocket\",\"status\": \"2\",\"message\":\"FFFFx0 Hash mismatch!  $i has changed!  Do not trust any block after and including $i\"}"
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



provideBlocks() {
        commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
        blockMessage="["
        for i in $(echo $jsonMessage  | jq -r '.result'| jq -c '.[]'| sed "s/\"//g"); do
                blockBase64=$(cat ${BLOCKPATH}/$i | base64 | tr '\n' ' ' | sed 's/ //g')
                blockMessage=$(echo "$blockMessage\"$blockBase64\",")
        done
        # remove last comma
        ${var::${#var}-1}
        blockMessage=$(echo ${blockMessage::${#blockMessage}-1})
        blockMessage=$(echo "$blockMessage]")
        #echo $blockMessage
        #echo "{\"command\": \"provideBlocks\",\"messageType\":\"direct\",\"destinationSocket\": \"$fromSocket\",\"status\": \"0\",\"result\":{\"blockList\":$blockMessage}}" 
        return="{\"command\": \"provideBlocks\",\"commandCode\":\"302\",\"messageType\":\"direct\",\"destinationSocket\": \"$fromSocket\",\"status\": \"0\",\"result\":$blockMessage}"
        AddBlockFromNetwork $return
        # Get list and parse JSON
        # if 30% then provide message to download full copy (optional)
}


removeTransactionsFromPending() {
    files=$1
    cat ${files[@]} | grep TX | while read tx; do
        sed -i "/$tx/d" $BLOCKPATH/blk.pending
    done
}


getNewBlockFromNode() {
    commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
    errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
    fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
    results=$(echo $jsonMessage | jq -r '.result')
    blocksTemp=$tempRootFolder/block_$RANDOM
    mkdir -p $blocksTemp
    count=0
    ERRORS=0
    for newBloks in  $(echo $jsonMessage | jq -r '.result' | jq -c .[]); do
        let " count = $count + 1 "
        echo $newBloks |sed "s/\"//g"| base64 -d > $blocksTemp/$count.blk 
        BlockID=$(cat $blocksTemp/$count.blk | grep BLOCKID | awk -v FS=':' '{ print $2}'| sed "s/ //g")

        if [ ${#BlockID} -ge 4 ] ; then
            echo date > $tempRootFolder/0001
            mv $blocksTemp/$count.blk $blocksTemp/$BlockID
        fi
        
        if [ $(cat ${blocksTemp}/${count}.blk | wc -l) -eq 3 ] && [[ $(cat ${blocksTemp}/${count}.blk | grep "Previous Block Hash") ]]; then
            continue
        fi

        if [ $(echo ${BlockID}| awk -v FS='.' '{print $3}') == "solved" ]; then
            echo date > $tempRootFolder/0002
            checkBlockSignature $blocksTemp/$BlockID
        else
            echo "{\"command\":\"getNewBlockFromNode\",\"destinationSocket\": \"$fromSocket\",\"messageType\":\"direct\",\"commandCode\":\"$errorCode\",\"status\":"2",\"message\":\"Cheating in chain, block sign issue\"}"
            exit 1
        fi

        cat $blocksTemp/$BlockID |  grep '^TX' |while read line; do
            echo date > $tempRootFolder/0003
            # STARTING NEW LOOP to read all TX
            validateTransactionMessage $line
            [ $? -ne 0 ] && 
                echo "{\"command\":\"$command\",\"status\":3,\"message\":\"Some transactions cannot be validated. it is destroy whole signature of block\"}" &&  
                exit 1
            done
    done

    
    expcetedNtwrkBlockID=$(ls -1 ${BLOCKPATH}  | sort -n | grep "\.solved$" | sort -n |tail -n 1| awk -v FS='.blk.solved' '{print $1+1}')
    comingNtwrkBlockID=$(ls -1 ${blocksTemp}   | sort -n | grep "\.solved$" | sort -n |head -1  | awk -v FS='.blk.solved' '{print $1}')
    
    ## change blk file to current+1 for validataion chain
    [ $(ls ${blocksTemp}/*.blk | wc -l) -eq 1 ] && mv ${blocksTemp}/*.blk ${blocksTemp}/$(expr ${comingNtwrkBlockID} + 1).blk || exit 1

    echo "$(date)" > $tempRootFolder/0004

    if [ "$expcetedNtwrkBlockID" == "$comingNtwrkBlockID" ];then
        echo date > $tempRootFolder/0005
        validateNetworkBlockHash "$blocksTemp"
        ret=$?
        ecoh $ret > $tempRootFolder/ret
            if [ $ret -eq 0 ]; then
                echo date > $tempRootFolder/0007
                blocks=$(ls ${blocksTemp}/*blk.solved*)
                removeTransactionsFromPending $blocks
                mv $blocksTemp/*blk.solved $BLOCKPATH/
                echo "{\"command\":\"notification\",\"status\":0,\"commandCode\":\"001\",\"messageType\":\"broadcast\",\"result\":[$results]}"
            fi
    else
            echo "{\"command\":\"getNewBlockFromNode\",\"commandCode\":\"$errorCode\",\"messageType\":\"direct\",\"status\":"2",\"destinationSocket\":\"$fromSocket\",\"message\":\"Function: validateNetworkBlockHash , Folder:$blocksTemp, firstFileNetwID=$firstFileNetwID and lastFileCurrIDplus1=$lastFileCurrIDplus1 Chain ID $BlockID is not matching\"}"
    fi

    # rm -rf $blocksTemp

}
