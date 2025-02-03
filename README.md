# MovementRandomizer
Randomizes and shuffles most movement values in Titanfall 2!
<br><br>

# Usage
* Grab an exe from the [releases](https://github.com/Nextracer1/MovementRandomizer/releases) page
* Run TF2MovementRandomizer.exe as administrator 
* Input your titanfall 2 install directory
* In-game, run "exec randomizer" in console.
(you can get a console through anything like icepick, SRMM, ronin launcher, northstar, etc.)
<br><br>



# Customization
config.txt contains every convar that gets randomized, with a min range followed by a max range right next to them. You can customize the randomizer by adding/removing whatever convars you want and tweaking their ranges.

`convar_name minrange maxrange`


<br>By default, everything will be treated as a decimal range. Putting a `!` before a convar makes it an integer instead. For example, the convar `autosprint_type` can only be changed to an integer.

`!convar_name minrange maxrange`


<br>Putting a `*` between two sets of min/max ranges means that there will be a coinflip between the two ranges. For example, `sv_gravity 10 100* 20 200` will mean that it will either be randomized from 10-100 OR from 20-200 with a 50/50 chance. This is useful for when you have a convar that you want to have a higher range in while still making it likely to encounter a good spread of values.

`convar_name minrange1 maxrange1* minrange2 maxrange2`



<br>Make sure you do this syntax exactly as it is here, because the program is VERY delicate when it comes to reading the file lol<br>
If you do something differently it'll probably either crash or make the convar fail to change in-game
