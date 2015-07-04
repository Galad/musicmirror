Feature: Transcode
	As a user
	I want to transcode files from one folder to another

Scenario Outline: Transcode File
	Given the source directory is "Music Source Path"
	And the target directory is "Music Target Path"
	And the file "<source file>" is in the source path
	When I save the settings
	Then the target path should contains the file "<target file>"

Examples: 
	| source file  | target file |
	| Mp3File.mp3  | Mp3File.mp3 |
	| Mp3File.flac | Mp3File.mp3 |

Scenario Outline: Transcode File in subfolder
	Given the source directory is "Music Source Path"
	And the target directory is "Music Target Path"
	And the file "<source file>" is in the source path subfolder "Source Path Subfolder"
	When I save the settings
	Then the target path with the subfolder "Source Path Subfolder" should contains the file "<target file>"

Examples: 
	| source file  | target file |
	| Mp3File.mp3  | Mp3File.mp3 |
	| FlacFile.flac | FlacFile.mp3 |
