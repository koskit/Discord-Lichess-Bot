
# What is Discord-Lichess-Bot

First of all, welcome. This repository contains an implementation on a .net core application that "connects" to discord and lichess, and "keeps" and eye on a lichess team. Each *x* seconds, it requests the statuses of the team members and sends a message on discord if a member came online or disconnected from the site.

Something really close to "your *x* friend just came online" that we see in game platforms (such as blizzard client or steam) but for lichess.

The current implementation supports only teams. That means that you can not select *specific* players, but only a single team. The program iterates through the team members and posts updates on their status.

## Usage
The most easy way to use the bot is in a container running the image that can be found here:

``
https://hub.docker.com/r/koskit/discordlichessbot
``

On the other hand, you can build the solution on your machine locally, and simply start the application (although you must pass the environment variables). If that feature is requested, I will implement it (local execution outside docker).

## Running the docker image

You can run the following command that contains all the needed environment variables.
```bash
$ docker run -d \
    --name discord-lichess-bot \
    -e JOB_ITERATION_SLEEP_DURATION=30000 \
    -e JOB_REPORT_TEAM_CHANGES=true \
    -e LICHESS_PAT="xxx" \
    -e LICHESS_TEAM_NAME="xxx" \
    -e DISCORD_SERVER_NAME="xxx" \
    -e DISCORD_SERVER_TEXT_CHANNEL="xxx" \
    -e DISCORD_BOT_TOKEN="xxx" \
    koskit/discordlichessbot
```

### Variables explanation


- ``JOB_ITERATION_SLEEP_DURATION `` How much *ms* the program will sleep between each report. This is used so we do not run into lichess api limits (how many api calls/minute etc). The current implementation, only contacts lichess ONCE (one api call, ``GetTeamMembers()``) per reporting. **Important detail**: This *sleep duration* is not in a scheduled manner. If the reporting starts at *T* time, finishes at (e.g.) *T+15* seconds, then it sleeps for 30 seconds, and the next reporting will start at *T+45* seconds, and **not** on *T+30*. In essence, it waits for *x* seconds between each report. ​
- ``JOB_REPORT_TEAM_CHANGES`` Bool that indicates if the bot should announce if a member joined or left the team. ``{memberUsername} joined/left the {TeamName} team!``
- ``LICHESS_PAT`` Personal Access Token for lichess website. You can find out more [here (click me)](https://lichess.org/api#section/Authentication).
- ``LICHESS_TEAM_NAME`` The name of your lichess team.
- ``DISCORD_SERVER_NAME`` The name of the discord server (if your bot app gets invited to more than one).
- ``DISCORD_SERVER_TEXT_CHANNEL`` The channel name (text, not voice) of your server.
- ``DISCORD_BOT_TOKEN`` The token of your discord bot.


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
MIT License

Copyright (c) 2021 [koskit](https://github.com/koskit)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.