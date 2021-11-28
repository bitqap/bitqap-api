mapFunction2Code () {
        ######################################################################################################
        #                       2021.11.22
        # This function designed to return function code.
        # this function will be used on Notification. in "notification" command bashCoin.sh don't do anything.
        # because of command bashCoin return command itself on result. 
        # But code will help us to execute some action.

        funcName=$1
        type=$2
        case "$funcName" in 
                mine|100)
                                code=100
                                name=mine
                                ;;
                mineGenesis|101)
                                code=101
                                name=mineGenesis
                                ;;
                checkAccountBal|200)
                                code=200
                                name=checkAccountBal
                                ;;
                getTransactionMessageForSign|201)
                                code=201
                                name=getTransactionMessageForSign
                                ;;
                pushSignedMessageToPending|202)
                                code=202
                                name=pushSignedMessageToPending
                                ;;
                AddBlockFromNetwork|302)
                                code=302
                                name=AddBlockFromNetwork
                                ;;
                listNewBlock|300)
                                code=300
                                name=listNewBlock
                                ;;
                provideBlocks|301)
                                code=301
                                name=provideBlocks
                                ;;
                updateNetworkInfo|401)
                                code=401
                                name=updateNetworkInfo
                                ;;
                *)
                                code=000
                                ;;
        esac
        
        if [ "$type" == "code" ]; then
          echo $code
        fi

        if [ "$type" == "func" ]; then
          echo $name
        fi
}


mapERRORFunction2Code () {
        funcName=$1
        type=$2
        case "$funcName" in 
                mine)
                                code=500
                                name=mine
                                ;;
                mineGenesis)
                                code=501
                                name=mineGenesis
                                ;;
                checkAccountBal)
                                code=510
                                name=checkAccountBal
                                ;;
                getTransactionMessageForSign)
                                code=511
                                name=getTransactionMessageForSign
                                ;;
                pushSignedMessageToPending)
                                code=512
                                name=pushSignedMessageToPending
                                ;;
                AddBlockFromNetwork)
                                code=520
                                name=AddBlockFromNetwork
                                ;;
                listNewBlock)
                                code=521
                                name=listNewBlock
                                ;;
                provideBlocks)
                                code=522
                                name=provideBlocks
                                ;;
                validateNetworkBlockHash)
                                code=523
                                name=validateNetworkBlockHash
                                ;;
                updateNetworkInfo)
                                code=401
                                name=updateNetworkInfo
                                ;;
                *)
                                code=000
                                ;;
        echo $code
}