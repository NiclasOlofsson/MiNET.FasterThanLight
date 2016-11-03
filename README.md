# MiNET Faster Than Light (FTL)

MiNET FTL is an example implementation of a proxy server intended to be used to transfer players between a set of MiNET running in *node* mode. The FTL server communicates keeps the session with MCPE clients using a proxy, and forwards traffic to various MiNET nodes dealing with the regular MiNET game logic.

Even though FTL use TCP to communicate between proxy and node, any means of communication can be used.

MiNET FTL is only a proof of concept implementation, mostly used to test the actual API provided by MiNET itself, and should NOT be used for production purposes of any kind. Code is provided AS IS and no support will be provided.
