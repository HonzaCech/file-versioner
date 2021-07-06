# File versioner
This program reads a given path and detects changes in it
I have decided to use JSON file named ".versions" stored in the path to store neccesary info about the files. If there is none such file in the directory, it is a new one and the program is running for the first time. I detect changes based on a change in MD5 checksum. I consider this to be more robust than detecting it through `File.GetLastWriteTime`, since this sometimes behaves strangely and would create "false positives" when changing file and then changing it back to the original version between two runs of my program. It is, of course, slower.
Limitations and possible improvements:
- UI is not the greatest one
- It would be nice to have input textbox changed into something for selecting path, but that is beyond my Razor skills
- Displaying errors should be nicer, but I didn't consider it worth so much time that I would have to spend on it, since my frontend skills are quite low
- The "FileVersioningBackend" should be a separate project, but it seemed not worth it for such a small thing
- I'm transfering list of versioned files including their hashes to frontend, and the hashes are not needed there. They do not take too much space now, but removing the hashes would be a possible optimalization if the transfers would not be only "local" as they are in this task.
