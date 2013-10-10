ELIoT# mini
==========================

This is a small library and application developed for the Netduino 1 to show the compatibility between devices running the [ELIot](http://home.dei.polimi.it/sivieri) framework and low-powered devices.

It contains a simple example of a scenario we developed for ELIoT (smart home), which uses the [Erlang C# implementation](https://github.com/takayuki/Erlang.NET) adapted to our framework: this simply means (for this small example) that we do not use the concept of Erlang node and authentication; our network protocol is implemented directly into the appliance example. The Erlang C# implementation is licensed under the EPL license.
The project also contains an implementation of SHA1, taken from the Mono source code, licensed under the MIT license.

The behavior of the appliance is to receive a beacon message on the network from the control panel (using broadcast), de-serialize it, serialize the answer (which contains a compiled Erlang blob file) and send it back to the caller.
