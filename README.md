# What This Code Does

This is a set of Python Scripts that help you to gather data of Players from the top 5 leagues across seasons.

It also conducts Simulations based on a player's xG to determine the 'Most Probable' Goal tally for a particular season and compare it to the Real tally of Goals by that player to determine how sustainable his numbers were.

# Requirements

To install the required dependencies run the following in terminal: `pip install -r requirements.txt`

# Running The Code

## Step 1:

Run the PlayerFinder.py script to generate the latest PlayerIDs.csv : `python -u PlayerFinder.py`

## Step 2:

After PlayerIDs.csv is generated, run the PlayerSeasonsStats.py script to search for the Player of your choice: `python -u PlayerSeasonsStats.py`

## Please Notes:

Step 1 is not required as PlayerIds.csv is already provided.
