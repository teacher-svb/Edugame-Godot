Intro
In my game Lexis Bell, I have a lot of
UI and pretty much everything is
animated. I received a lot of questions
about it. So, I thought I could make one
video where I share the tips and tricks
I use to animate UI relatively easily in
GDO. Note that I'm not using GDO 4.7,
which comes with a bunch of new
features, but if you are interested, I
made a video about it linked in the
description. You might be wondering how
people make juicy games where
everything, every little button and
texts are animated. The trick is to make
pre-made and reusable tween you can just
drop on your elements. I'm going to show
you how I reuse animations, how I
animate the UI elements such as inside
containers, and a little tricks to make
that easier. In this video, I'm only
using twins as I believe it's the
easiest and best method for UI. Sorry if
you like the animation player. Let's
start with animating buttons.
Buttons
One of the first thing I animate in
games is buttons. you use them all the
time and I feel like it's pretty
important to have cool buttons. Of
course, they need to match the style of
your game. In my games, I would call it
cartoony. By the way, if you're
interested about the video where I talk
about different kinds of juice and which
one to use depending on your game genre,
please let me know in the comments. I'd
love to talk about that. So, my buttons
are pretty simple. I animate both the
hover and hover state by doing two
things, using scale and rotation. For
scale, I separate the X and Y components
as I tween them to give a little bit of
squash and stretch effect. This makes
the scaling a bit more goofy and
satisfying. On top of that, I tween the
rotation in a random direction and then
I come back to zero. Two things for the
rotation. Don't rotate too much as you
could trigger the mouse exiting on the
button. And the second thing is separate
the range of rotation from the
direction. This is what this line is
doing. The minus1 one in an array.pick
Pick random is to decide in which
direction I'm rotating. And then the
range range is for the amplitude. If you
simply do range range minus 10 to 10,
for example, you'll have a lot of
rotation that are close to zero. So,
it's better to select the range you want
in the positive and then just apply the
direction or rotation you want. On these
twins, I use the transition back and
ease out. This is a very punchy curve
with a slight overshoot at the end,
which feels great for cartoony
over-the-top juice. I really recommend
looking at the cheat sheet and getting
used to understanding what curves to use
in what situation. I personally use ease
out and trends back a lot when I want a
punchy cartoony effect. I play a simple
SFX when hovering but not when leaving
the button. This is to not overwhelm the
player too much as we really care about
the feedback when we enter the button.
With that, we have a simple, very simple
in fact button. On my basic button, I'm
not doing anything else. But you can
imagine changing the texture, animating
the text, and tons of other things. You
can kind of go crazy with buttons. Bonus
Bonus tip
tip. One thing I'd like to do with my
buttons is scale the power of the
animation depending on the size. So I
define a size at which the animation
should be at full power and then I
simply scale it depending on the size of
the button. So the longer the buttons
get, the scale is going to get reduced.
This is super important especially with
rotation as otherwise a long button will
introduce a lot of rotation and the
longer your button is the more the sides
are going to travel because of how
angles work. So I really recommend
applying this kind of scale if you're
going to reuse your elements in a bunch
of different places with a lot of
different sizes.
Containers
Speaking of buttons, one easy trick to
make your UI more alive is to animate
the content of containers. And for
example, your button containers. Let's
look at my title screen for that. I have
a bunch of buttons and I want them to
animate when we arrive there.
In this case, it's pretty simple. And
the ready function will go through the
children and make them transparent. It's
important to note we are making them
transparent and we don't touch the
visible because containers will
recalculate the transform of their
children if something changes
visibility. Making an element not
visible makes it not count for the
container logic and that's not what we
want. We want to make it transparent.
Then I simply do a tween on all the
elements to make them move or scale.
Here you do what you prefer. To make it
smooth, it can be a good idea to have a
little delay before you start the button
animations so your scene is loaded and
visible. And also if everything animates
at the same time, it can look busy and
not pleasing. That's why I also delay my
tween between the elements of the
container. And here my preferred trick,
especially for snappy animation for
something like buttons, is to make all
the twins parallels and use a little set
of delay that increases slightly so they
don't animate at the same time, but
there's a small overlap. This feels less
robotic than having elements animate one
after the other. Keep these animations
snappy so people can navigate fast.
There's nothing worse than a slow UI
because you have to wait for animations
to finish. One way to make it work well
is to give focus or input as soon as the
button is in the scene even if the
animation is not finished. For most of
your UI, this should work well and fast
players will be able to navigate fast
even if the button is not properly shown
yet. To make this reusable, I simply
have a script I can drop on any
container and it's automatically
animated whenever the node is ready. I
have a few predefined animations such as
sliding in or scaling and that's enough
for the most case. The game I use to
Wishlist Lexispell!
showcase my animation is Lexi Spell, a
word game rogike that has a cool physics
twist. If you are interested in this
kind of game, there's a demo on Steam.
You can wishlist the game and it's
probably going to release relatively
soon when I publish this video.
Overriding _set
Another little trick I like when you
want to add a simple animation to
something that was not designed for.
Think about a simple text that is
updated and doesn't have a script. You
can override the set function. This is
an internal function that is called when
you change a property value. And here
you can do a bunch of stuff. In my case,
I'm simply adding a little bump tween to
make the text scale and rotate as it
changes value. You can also make the
text count up or down if it was a
number. For example, you can see the
code on screen. It's relatively simple,
and the GDO documentation explains how
to deal with this function. You can make
a little reusable script and drop it on
the node you want to animate. Use an
export variable to specify the name of
the property you want to track. And
that's it. You have an autoan animated
AutoTween
element.
An important trick to animate faster is
to make twins reusable. I showed that
already with the automatic tween for
containers, but I also do it in two
different ways. I call them autot twins
and tween ones. Autoteen is a script
which has a target and it will apply a
tween automatically or manually if I
really want to. This is especially
useful to retrofit animation into
existing codebase. For example, let's
say you have a node which you show or
hide during gameplay. During
prototyping, you simply change the
visibility, but now you want it to be
animated. Instead of changing code, you
put the auto tween as a child, set the
target of the various property, and you
set the trigger to visibility. Now,
whenever you change the visibility, the
autoteen will overwrite that and apply a
tween in order to animate the node. It's
something I use a lot and makes simple
animation very easy. It's still a work
in progress. I could make it way more
complex with various effects or
different triggers like other tween or
properties. Still, the automatic
tweening on visibility is incredibly
useful.
TweenOnce
The other simpler tween is tween once.
Again, it's a simple tween effect that I
often use. This time triggered manually.
It's useful to make objects react once,
for example, when you use them or when
their value changes. If you update them
as you work on your game, you could end
up with a bunch of predefined effects
that allow you to animate most things
very fast. A big advantage of doing that
is that makes your game feel more
coherent. The same way you use the same
color throughout your game, here you
reuse the same effects. It creates
coherence and it helps defining a
language for your game.
Conclusion
As you can see, animating UI is not that
hard. The general rule I follow is to
animate anything that changes. If
something appears, disappears, or
changes value, it needs to be animated.
The amount of animation or how
exaggerated you want depends on the mood
of your game. You want to keep your
animation snappy not to get in the way
of players. When animating, you layer
different things, and you don't make
things move all at once to avoid a
blocky feel.
I hope you found this interesting. I
have more tips and tricks for UI coming
if you're interested. So, please let me
know in the comments. And if you're
really struggling with that or you want
to work with me, I have consulting
sessions and I do freelance for juice
and UI. So don't hesitate to hit me up.
Link in the description.
These videos are made possible by my
supporters on Patreon. I want to thank
them for supporting my work. And if you
want, you can join my Patreon, too. It
really helps. Check out the link in the
description. If you want to support me
further, you can check out my other
games on Steam. I'll see you in the next
video. Bye.