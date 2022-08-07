# Import all the Modules and Packages

from urllib import response
import requests
from bs4 import BeautifulSoup
import json
import pandas as pd
import random
import math
import sys

pid  = pd.read_csv('PlayerIDs.csv')

while(True):
    while(True):
        name = str(input('Player Name: '))
        i = 0
        l = []
        m = []
        count = 0
        while(i<pid.shape[0]):
            if name.lower() in str(pid.iat[i, 1]).lower():
                l.append(i)
                m.append(pid.iat[i, 0])
            i+=1

        if len(l) == 0:
            print("No such name exists!")
            continue
        elif len(l) == 1:
            break
        else:
            for j in l:
                print(f'{pid.iat[j, 1]} ({pid.iat[j, 2]}) : {count+1}')
                count+=1
            break

    # Get the Player ID to create a URL

    base_url = 'https://understat.com/player/'
    if(len(m) == 1):
        choice = 1
    else:
        choice = int(input('Enter your choice: '))
    print(f'The Stats of {pid.iat[l[choice-1], 1]} are: ')
    player_id = str(m[choice-1])
    url = base_url+player_id
    res = requests.get(url)
    soup = BeautifulSoup(res.content,'lxml')
    scripts = soup.find_all('script')

    # Get everything under the 'scripts' tag

    strings = scripts[1].string

    # Slices unnecessory elements and create a JSON

    ind_start = strings.index('(')+2
    ind_end = strings.index("')")
    json_data = strings[ind_start:ind_end]
    json_data = json_data.encode('utf8').decode('unicode_escape')
    data = json.loads(json_data)

    # Converts the JSON data into a Data Frame

    games = []
    goals = []
    shots = []
    time = []
    xG = []
    season = []
    team = []
    assists = []
    xA = []

    data_season = data['season']

    for index in range(len(data_season)):
        for key in data_season[index]:
            if key == 'games':
                games.append(data_season[index][key])
            if key == 'goals':
                goals.append(data_season[index][key])
            if key == 'shots':
                shots.append(data_season[index][key])
            if key == 'time':
                time.append(data_season[index][key])
            if key == 'xG':
                xG.append(data_season[index][key])
            if key == 'season':
                season.append(data_season[index][key])
            if key == 'team':
                team.append(data_season[index][key])
            if key == 'assists':
                assists.append(data_season[index][key])
            if key == 'xA':
                xA.append(data_season[index][key])


    # Creates the Data Frame

    col_names = ['Season', 'Team', 'Games', 'Time', 'Shots', 'xG', 'Goals', 'xA', 'Assists']
    df = pd.DataFrame([season, team, games, time, shots, xG, goals, xA, assists], index = col_names)
    df = df.T

    rows = df.shape[0]
    i = 0
    xG_per_shots = []
    while(i<rows):
        try:
            xG_per_shots.append(float(df.iat[i, 5])/float(df.iat[i, 4]))
            i+=1
        except ZeroDivisionError as e:
            xG_per_shots.append(0)
            i+=1
    df['xG per Shots'] = xG_per_shots

    average = []
    i = 0
    while(i<rows):
        highest = 0
        lowest = 1000
        xG_value = float(df.iat[i, 9])
        j = 0
        total = 0
        while(j<20):
            sum = 0
            k = int(df.iat[i, 4])
            p = int(xG_value*1000)
            l = 0
            while(l<k):
                goal = random.randint(0, 1000)
                if(goal<p):
                    sum+=1
                l+=1
            total+=sum
            j+=1
        avg = int(total/20)
        average.append(avg)
        i+=1
    df['Median Goals based on Simulation'] = average

    sus = []
    i = 0
    while(i<rows):
        n = int(df.iat[i, 4])
        x = int(df.iat[i, 6])
        xi = int(df.iat[i, 10])
        if n == 0:
            p = 0
        else: 
            p = int(df.iat[i, 10])/n
        q = 1-p
        ncx = math.factorial(n)/(math.factorial(x)*math.factorial(n-x))
        ncxi = math.factorial(n)/(math.factorial(xi)*math.factorial(n-xi))
        sustain = ncx*(p**x)*(q**(n-x))
        sustaini = ncxi*(p**xi)*(q**(n-xi))
        diff = sustaini - sustain
        sustain = 100-(diff/sustaini*100)
        sus.append(sustain)
        i+=1

    df["Goal Sustainability %"] = sus


    print(df)
    response = input('Want to Look For Another Player? (y/n): ')
    if(response == 'n'):
        sys.exit()
    else:
        continue
