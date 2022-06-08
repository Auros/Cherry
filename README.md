# Cherry
A lightweight chat-based song request system for Beat Saber.

## Please Note
I did not make this mod as a replacement for other song request mods. I made this mainly for myself and a few other people who wanted a request mod that's more lightweight and streamlined.

Please **do not** request any features to this mod with "Can you add 'X feature' like how it is in 'Y request mod'?" I won't be taking new feature requests publicly*.

*The main exception is with Templating, if you have an idea for a property for request message templates, send me a DM on Discord @ `Auros#0001`

## Why?
Like said above, I wanted to make a song request manager that was more lightweight and with room for expandability.

## Capabilities
- Allows twitch chat to request songs using the !bsr <key> command.
- Filter By: Map Rating
- Filter By: Automapped Songs
- Filter By: Minimum Map Age
- Filter By: Map Song Length
- Filter By: Min/Max NJS
- Allow a maximum # of requests from specific user roles (Normal, Subscriber, VIP, Mod)
- Option to prefix all messages with ! for TTS bots
- !oops command

**PLEASE NOTE: THIS IS A KEY ONLY REQUEST MANAGER**

This means, people have to enter !bsr (key). This mod does not search by song name!
  
## Templating

  People with malicious intent will sometimes intentionally request songs with key words which will get the streamer banned or suspended on Twitch.
  To combat this, Cherry now by default uses a more safer request message but allows the user to change if if they want to.
  
  The current template properties are:
  * `%key`: The key of the map
  * `%requester.mention%`: The mention (for example @abcbadq) of the requester
  * `%map.uploader.name%`: The name of the person who uploaded the map
  * `%map.name%`: The name of the map
  
  You can easily add your own formats by editing the config file at `Beat Saber/UserData/Cherry.json`. In the "RequestMessageTemplates" field, add a new line with your desired template type.
