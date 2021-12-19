# [BITQAP]       [![bash][bash-img]][bash] [![python][python-img]][python] [![websocket][websocket-img]][websocket] [![lpipe][linuxPipe-img]][lpipe]

<img src="https://raw.githubusercontent.com/bitqap/info/main/source/images/favicon.ico" align="right"
     alt="BITQAP" width="178" height="178">

BITQAP is simple blockchain project on top of **BASH** and **Python** programming language. Project support main features crytocurrency.
 bash and python scripts binded each other by **Linux Named Pipe** Technology.

* **Mining** and **REWARD** support.
* Broadcasting **MINED BLOCKS** via neighbors. 
  Neighbors that, after being added to the local blockchain, also broadcast to their neighbors.
* **Coin** sending to PubKeyHash(SHA256) wallet address by Private Key signature.
* **Transaction** Boradcasing via Neighbors.
* Validation of mined **BLOCKS** and **Transactions** before adding BLOCKCHAIN and Pending transaction accordingly. 

<p align="center">
  <img src="https://raw.githubusercontent.com/bitqap/info/main/source/images/logo1.png" alt="BLOCHCHAIN" width="738">
</p>


`jq` and `bc` tool need to be installed on Linux system.

```
apt-get update
apt-get install jq bc
```


`websocket_server` and `websocket-client` package need to be installed on Python3.

```
pip3 install websocket_server
pip3 install websocker-client
```


[BITQAP]:          https://bitqap.github.io/info
[bash-img]:        https://github.com/bitqap/bitqap/blob/main/doc/img/bash_readme_s6.jpeg
[bash]:            https://en.wikipedia.org/wiki/Bash_(Unix_shell)


[python-img]:      https://github.com/bitqap/bitqap/blob/main/doc/img/python_icon_s4.jpeg     
[python]:          https://en.wikipedia.org/wiki/Python_(programming_language)
 
[websocket-img]:   https://github.com/bitqap/bitqap/blob/main/doc/img/ws1_s4.jpeg
[websocket]:       https://en.wikipedia.org/wiki/WebSocket

[linuxPipe-img]:   https://github.com/bitqap/bitqap/blob/main/doc/img/npipe_icon_s6.jpeg
[lpipe]:           https://en.wikipedia.org/wiki/Named_pipe

