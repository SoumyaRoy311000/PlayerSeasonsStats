# Import all the Modules and Packages

import requests
from bs4 import BeautifulSoup
import json
import pandas as pd

base_url = 'https://understat.com/player/'
player_name = []
player_id = []
player_team = []
i = 1
while i<10000:
    try:
        j = str(i)
        url = base_url+j
        res = requests.get(url)
        soup = BeautifulSoup(res.content,'lxml')
        teams = soup.find_all('script')
        names = soup.find_all('title')
        strings = teams[1].string
        name = str(names)
        ind_start = name.index('<title>')+7
        ind_end = name.index(' |')
        name = name[ind_start:ind_end]
        index_start = strings.index('(')+2
        index_end = strings.index("')")
        json_data = strings[index_start:index_end]
        json_data = json_data.encode('utf8').decode('unicode_escape')
        data = json.loads(json_data)
        team = []
        data_season = data['season']
        for index in range(len(data_season)):
            for key in data_season[index]:
                if key == 'team':
                    team.append(data_season[index][key])
        Displayed_team = str(team[0])
        if name == '':
            i+=1
            continue
        else:
            player_name.append(name)
            player_id.append(j)
            player_team.append(Displayed_team)
            i+=1
    except Exception as e:
        i+=1
        continue


col_names = ['Player ID', 'Player Name', 'Team']
df = pd.DataFrame([player_id, player_name, player_team], index = col_names)
df = df.T

df.to_csv('PlayerIDs.csv', index= False)


