# THIS FUNCTIONS FOR IP PEERS

ping() {
	fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
	echo "{\"command\":\"ping\",\"messageType\":\"direct\",\"from\":\"$(hostname)\",\"status\":0}"
}


initializeDB() {
	# create table if not exist. 
	# UNIQUE fields
	echo "CREATE TABLE IF NOT EXISTS nodes (node1 TEXT, node2 TEXT, weight1 text, weight2 text, hash1 text, hash2 text);"| sqlite3 $DB
}


addConnectionPeers() {
	commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
	# errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
	# adding new connection to Node.
	# constrait. If not exist.
	node1=$(echo "${jsonMessage}"  | jq -r '.node1')
	node2=$(echo "${jsonMessage}"  | jq -r '.node2')
	echo "insert into nodes (node1,node2) values ('$node1','$node2');" | sqlite3 $DB
	[ $? -eq 0 ] &&
		echo "{\"command\":\"addConnectionPeers\",\"messageType\":\"broadcast\",\"from\":\"$(hostname)\",\"status\":0,\"tag\":\"FFFFx0\",\"node1\":\"$node1\",\"node2\":\"$node2\"}" ||
		exit 1
}


removeConnectionPeers() {
	commandCode=$(mapFunction2Code ${FUNCNAME[0]} code)
	# errorCode=$(mapERRORFunction2Code ${FUNCNAME[0]})
	# removing connection from node
	leftConnection=$(echo "${jsonMessage}"  | jq -r '.node')
	echo "delete from nodes where node1='$leftConnection' or node2='$leftConnection';" | sqlite3 $DB
	[ $? -eq 0 ] &&
		echo "{\"command\":\"removeConnectionPeers\",\"messageType\":\"broadcast\",\"from\":\"$(hostname)\",\"status\":0,\"tag\":\"FFFFx0\",\"node\":\"$leftConnection\"}" ||
		exit 1
}

peerInfo() {
	fromSocket=$(echo ${jsonMessage}  | jq -r '.socketID')
	myip=$(echo ${jsonMessage}  | jq -r '.myip')
	[ "$fromSocket" == "null" ] && fromSocket=""
	msg={\"command\":\"peerInfo\",\"messageType\":\"direct\",\"status\":0,\"destinationSocket\":$fromSocket}
	# sqlite JSON format export
	PeersList=$(sqlite3 -header \
						-json \
						$DB \
							"select 
								peer,
								count(1) as weight 
								from 
								(
									select node1 as peer from nodes where node1 !='$myip'
							 	 	union all 
							 	 	select node2 as peer from nodes where node2 !='$myip'
							 	)  
							 	group by peer;" | tr '\n' ' '| sed 's/ //g')
	
	#PeersList=$(echo ${PeersList}}| jq --arg dt $PeersList '. + [ $dt ]')
	[ ${#PeersList} -eq 0 ] && PeersList=[]
	msg={\"command\":\"peerInfo\",\"messageType\":\"direct\",\"destinationSocket\":$fromSocket,\"status\":0,\"result\":$PeersList}
	echo $msg
}



