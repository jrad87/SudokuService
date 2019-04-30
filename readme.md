To use this service:

Step 1: create a directory on your machine that you will be using. 

Step 2: Enter the path for that directory on line 55 in MyNewService.cs. Then build the application.

Step 3: To install it using powershell run installUtil from the build path of the service (./WindowsService1/bin/Debug)

Step 4: When the service is running, create a new text file in the directory you created in step 1

Step 5: Edit the text file in the follwing manner:
Type SUDOKU on the first line
On subsequent lines, enter each row from a sudoku puzzle, with zeros representing blanks

Example :
SUDOKU 
050004600
018700000
062050800
080003000
100000002
000100050
006070530
000002970
003600020

After you save the file and close it, reopen and it the puzzle will be solved.

