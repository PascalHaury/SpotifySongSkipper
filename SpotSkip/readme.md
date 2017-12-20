Who doesn't know it. Everyone has some artists or songs who you hate. 
Spotify hasn't implemented a feature where you can remove an artist or 
a song from your life... But here comes SpotSkip!

![alt tag](https://raw.githubusercontent.com/theHaury/SpotifySongSkipper/master/SpotSkip/readme_pictures/SpotSkip.PNG "SpotSkip Maing Window")

SpotSkip lets you choose what you want to hear.
It creates a block-list in your MyDocuments/SpotSkip Folder. If a 
song/artist/combo is playing which is listed in the block-list, 
SpotSkip will simply skip the Song!

Features:

- Block Songs

	You Don't like a Song and all of it's remixes/Covers.
	It blocks all songs that contain the song-title.
	
- Block Artist

    You don't like a certain Artist
	It blocks all songs from the current artist.
	
- Block Combo

    You like the song and the artist, but not a certain 
	remix/cover.
	It Blocks the current song from the current artist.
	
- BlockListManager(BLM)

    You accidentally blocked something? - No Problem!
	With the BlockListManager you can view your BlockList
	sorted by song, artist and combo. Simply select the 
	entry you want to unblock and press unblock. The Song
	will now no longer be skipped.
	
	![alt tag](https://raw.githubusercontent.com/theHaury/SpotifySongSkipper/master/SpotSkip/readme_pictures/BlockListManager.PNG "SpotSkip BlockListManager")
	
What you should know:

    SpotSkip works at the moment only with Spotify Desktop or the
    Windows 10 App. You should also close all other Multimedia software, 
    because if SpotSkip skips a song, it sends a Next Song Multimedia 
    key command to the OS. If any other Multimedia software is running in
    the Background, it may get triggered and starts playing something.
    In a (at the moment) very far future it is planned 
    to create an Android App, and Implement the Spotify API. 
	
How it works:

    SpotSkip reads the title of the Spotify window. The Title 
    contains the Song and the Artist. Now SpotSkip processes
    the string and searches for it in the BlockList. If it 
    finds the current song/artist/combo in the BlockList
    SpotSkip simulates a "Next Song" Multimedia button. 
