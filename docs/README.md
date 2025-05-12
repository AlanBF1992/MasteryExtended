# Mastery Extended

Mastery Extended is a mod for Stardew Valley that aims to allow the player more options to use the Mastery system.

## Gaining Mastery Points

You can gain Mastery Points in the same way than in vanilla Stardew, but now you can start gaining them after you get level 10 in the corresponding skill, not only after getting level 10 in all of them.


## Accessing the Mastery Cave

Other than the normal method, you can also get access if you get to Mastery Level 5 (*configurable*).

![](images/mastery-access.png)

## Where to invest

After getting access to the Mastery Cave, you can click on the pedestal to show the menu. 

![](images/pedestal-invest.png)

Clicking the "Invest" button will show you the Skills Menu.

![](images/skill-menu.png)

Clicking on one of the Skills will show you the Professions for that Skill.

![](images/professions-menu.png)

Clicking on a Profession will add it to your character.

![](images/profession-added.png)

## Claiming Mastery Rewards
You can't unlock it immediately as before. Now you need to unlock at least one extra profession at the Pedestal.


## Config
Hint: You should really use GMCM.
After the first run, there will be a `config.json` file in the mod folder. You can edit the following settings:

Setting                        | How it works
------------------------------ | ------
`MasteryExpPerLevel`           | Amount of experience points required for an extra Mastery Level after level 5. Default `30000`.
`MasteryRequiredForCave`       | Mastery Levels required to access the Mastery Cave. Default `5`. 
`MasteryCaveAlternateOpening`  | If you can access the Mastery Cave with Mastery Levels. Default `true`.
`MasteryRequiredForCave`       | Mastery Levels required to access the Mastery Cave (if enabled). Default `5`. 
`SkillNameOnMenuHover`         | Show the Skill Name when hovering on Professions in the Skill Menu. Default `true`. 
`PillarsVsProfessions`         | How Professions and Pillars interact. Default `Professions required for Pillars`. Other options: `Pillars required for Pedestal`, `Neither`.
`RequiredProfessionForPillars` | How many professions you need unlocked for the Mastery Pillar. Default `3`. 
`RequiredPilarsToThePedestal`  | How many Mastery Pillar you need unlocked to unlock professions. Default `3`.

### WoL/VPP Config
Setting                        | How it works
------------------------------ | ------
`MasteryPercentage`            | When you are between level 10 and 19, this percentage of Experience will go to Mastery Exp and the rest to the Skill Exp. Default `20`.

If you don't want to share exp, just set it to 0.

### VPP Config

Setting                        | How it works
------------------------------ | ------
`Lvl10ProfessionsRequired`     | How many Level 10 Professions you need to be able to unlock a Level 15 Profession. Default `2`. 
`Lvl15ProfessionsRequired`     | How many Level 15 Professions you need to be able to unlock a Level 20 Profession. Default `4`. 

## Plans
* Add **Upgrades**, another way to spend Mastery Levels
