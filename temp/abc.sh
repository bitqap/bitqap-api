ROOTDIR="/root/bashCo1"
BLOCKPATH="$ROOTDIR/data/blocks"
tempRootFolder=$ROOTDIR/temp

validateNetworkBlockHash() {
        folder=$1
        curr=$(pwd)
        #[ -z $flagByPassFolder ] && flagByPassFolder=1 || flagByPassFolder=0
        # only in first attempt
        flagByPassFolder=1
        errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
        cd $folder
        # Check that there are blocks to validate
        getLastBlockToAdd=$(ls -1 $BLOCKPATH| grep solved$| sort -n| tail -n 1)
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
        done
        cd $curr
}

validateNetworkBlockHash /root/bashCo1/temp/block_6608 
