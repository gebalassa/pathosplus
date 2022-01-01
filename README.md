# PathOS+

## About

PathOS+ is a project exploring the potential of AI agents to stand in for human players in early-stage level testing for games with 3D navigation in a virtual world, alongside expert evaluation. 

#### Team

Samantha Stahlke - Founder

Atiya Nova - Lead Developer

Stevie Sansalone - Research Assistant, Developer

Dr. Pejman Mirza-Babaei - Research Supervisor

### Using PathOS+

PathOS+ is being developed as an open-source framework for [Unity](https://unity.com/). To use PathOS in your project, all you have to do is create some simple markup highlighting interactive objects in your level, and instantiate AI agents to wander around. PathOS+ operates on top of Unity's Navmesh system, and requires no modification to your existing game objects or scripts. Here's a screenshot of the framework in action:

![screenshot_ui2](https://user-images.githubusercontent.com/13160430/147801795-316e100d-f412-445b-8457-c7d0d51429b4.PNG)

Agents can be customized to reflect different player profiles - for instance, cautious newbies focused on exploration, hardcore completionists, or diehard adrenaline junkies looking for a fight. Agents will navigate based on these profiles, giving you an approximation of how different players will navigate through your game's world.

You can find the manual for PathOS+ [here](https://drive.google.com/open?id=1Q19IY_Xm924RNgSqcFsv3I-s80j7yL7W).

[screenshot_ui]: https://i.imgur.com/CqAFg4l.png "PathOS Runtime UI"

### PathOS+ Tabs
The PathOS+ Window (located in Window>PathOS+) has numerous tabs that each serve their own purpose. Here is a brief breakdown of what each one does:

#### Setup
The references to the in-scene PathOS+ Agent, PathOS+ Manager, and Screenshot Manager can be set here. 

#### Agent
The values for the selected PathOS+ Agent can be edited here, included their personality values.

#### Resource Values
The values of enemies/health potions can be set here, along with whether or not the simulation ends on player death.

#### Batching
Multiple agents can be simulate simultaneously here. 

#### Profiles
Agent personality profiles can be modified here. 

#### Manager
The level can be set up here, by using the markup tool to mark the different entities within the level. 

#### Visualization
Agent playthroughs can be recorded here. They can be loaded in as things such as heatmaps. 

#### Expert Evaluation
The findings during expert evaluation can be recorded in here. They can then be exported into a formatted excel sheet. This also gives an overview of the level map. 
