# THIS FUNCTIONS FOR IP PEERS

initializeDB() {
	# create table if not exist. 
	# UNIQUE fields
	echo "CREATE TABLE IF NOT EXISTS nodes (node1 TEXT, node2 TEXT, hash1 text, hash2 text);"| sqlite3 $DB
}

addConnectionPeers() {
	# adding new connection to Node.
	# constrait. If not exist.
	node1=$(echo "${jsonMessage}"  | jq -r '.node1')
	node2=$(echo "${jsonMessage}"  | jq -r '.node2')
	echo "insert into nodes (node1,node2) values ('$node1','$node2');" | sqlite3 $DB
	[ $? -eq 0 ] &&
		echo "{\"command\":\"addConnectionPeers\",\"messageType\":\"broadcast\",\"tag\":\"FFFFx0\",\"node1\":\"$node1\",\"node2\":\"$node2\"}" ||
		echo "{\"command\":\"addConnectionPeers\",\"messageType\":\"broadcast\"}"
}

removeConnectionPeers() {
	# removing connection from node
	leftConnection=$(echo "${jsonMessage}"  | jq -r '.node')
	echo "delete from nodes where node1='$leftConnection' or node2='$leftConnection';" | sqlite3 $DB
}

