'From MIT Squeak 0.9.4 (June 1, 2003) [No updates present.] on 11 February 2012 at 7:29:30 pm'!"Change Set:		NanoBoardAGWithMotorDate:			27 December 2011Author:			Kazuhiro ABE <abee@squeakland.jp>This changeset enables to use a NanoBoardAG as a WeDo and a ScratchBoard.It requires a NanoBoardAG with a sketch named NanoBoardAGWithMotor.pde and a DC motor."!Morph subclass: #ScratchFrameMorph	instanceVariableNames: 'topPane viewerPane scriptsPane stageFrame workPane titlePane libraryPane menuPanel stageButtonsPanel readoutPane logoMorph projectTitleMorph flagButton fillScreenFlag paintingInProgress projectDirectory projectName projectInfo author loginName loginPassword watcherPositions shuffledCostumeNames justSaved viewModeButtons viewMode lastViewMode viewModeButtonsPanel toolbarPanel lastWeDoPoll wedoIsOpen '	classVariableNames: 'Clipboard DefaultNotes DefaultSprite Fonts FontsXO IsXO ScratchServers ScratchSkin ScratchSkinXO TakeOverScreen UseErrorCatcher Version VersionDate VisibleDrives WorkpaneExtent '	poolDictionaries: ''	category: 'Scratch-UI-Panes'!Morph subclass: #SensorBoardMorph	instanceVariableNames: 'column titleMorph portName port readouts sensorValues currentState highByte useGoGoProtocol scratchBoardV3 reportRaw scanPorts scanState scanStartMSecs lastPollMSecs command '	classVariableNames: ''	poolDictionaries: ''	category: 'Scratch-UI-Panes'!!ScratchFrameMorph methodsFor: 'accessing' stamp: 'ka 12/26/2011 23:32'!frame	^ self! !!ScratchFrameMorph methodsFor: 'stepping' stamp: 'ka 12/26/2011 23:17'!checkForWeDo	"Check for WeDo, and show motor blocks if it is found."	"Note: Polling on Vista can take several hundred milliseconds, so reduce polling to just a few times per minute."	| now |	now _ Time millisecondClockValue.	(lastWeDoPoll isNil or: [lastWeDoPoll > now]) ifTrue: [lastWeDoPoll _ 0].	((now - lastWeDoPoll) < 15000) ifTrue: [^ wedoIsOpen == true]. "don't poll too often"	lastWeDoPoll _ now.	WeDoPlugin readInputs.	(wedoIsOpen _ WeDoPlugin isOpen) ifTrue: [		workPane showMotorBlocks ifTrue: [^ wedoIsOpen].		self showMotorBlocks.		WeDoPlugin readInputs].	workPane sensorBoard portIsOpen		ifTrue: 			[workPane showMotorBlocks ifTrue: [^ wedoIsOpen].			self showMotorBlocks].	^ wedoIsOpen! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/25/2011 15:24'!allMotorsReset	| sensorBoard |	sensorBoard _ (self ownerThatIsA: ScratchStageMorph) sensorBoard.	sensorBoard command: nil! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/27/2011 17:46'!motor: motorName direction: directionName	"Set the direction of the given motor."	| dir stage |	stage _ self ownerThatIsA: ScratchStageMorph.	stage owner frame checkForWeDo ifTrue: [		dir _ 0.		(directionName includesSubString: 'reverse') ifTrue: [dir _ 0].		(directionName includesSubString: 'this way') ifTrue: [dir _ 1].		(directionName includesSubString: 'that way') ifTrue: [dir _ -1].		'A' = motorName ifTrue: [WeDoPlugin motorADirection: dir].		'B' = motorName ifTrue: [WeDoPlugin motorBDirection: dir].		' ' = motorName ifTrue: [			WeDoPlugin motorADirection: dir.			WeDoPlugin motorBDirection: dir]].	stage sensorBoard motor: motorName direction: directionName! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/27/2011 17:36'!motorOff: motorName	"Turn the given motor off."	| stage |	stage _ self ownerThatIsA: ScratchStageMorph.	stage owner frame checkForWeDo ifTrue: [		'A' = motorName ifTrue: [WeDoPlugin motorAOn: false].		'B' = motorName ifTrue: [WeDoPlugin motorBOn: false].		' ' = motorName ifTrue: [			WeDoPlugin motorAOn: false.			WeDoPlugin motorBOn: false]].	stage sensorBoard motorOff: motorName! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/27/2011 17:44'!motorOn: motorName	"Turn the given motor on."	| stage |	stage _ self ownerThatIsA: ScratchStageMorph.	stage owner frame checkForWeDo ifTrue: [		'A' = motorName ifTrue: [WeDoPlugin motorAOn: true].		'B' = motorName ifTrue: [WeDoPlugin motorBOn: true].		' ' = motorName ifTrue: [			WeDoPlugin motorAOn: true.			WeDoPlugin motorBOn: true]].	stage sensorBoard motorOn: motorName ! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 8/10/2010 21:22'!motorOn: motorID For: secs elapsed: elapsedMSecs dummy: dummy	"Turn all motors on for the given number of seconds."	motorID ifNil: [  "first call, start motor"		self motorOn: ' '.		^ #' '].	self motorOn: motorID.	elapsedMSecs >= (1000 * secs) ifTrue: [self motorOff: motorID].! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/27/2011 17:45'!startMotor: motorName power: power 	| stage |	stage _ self ownerThatIsA: ScratchStageMorph.	stage owner frame checkForWeDo ifTrue: [		self motor: motorName power: power.		self motorOn: motorName.].		stage sensorBoard motorOn: motorName power: power! !!ScriptableScratchMorph methodsFor: 'motor ops' stamp: 'ka 12/25/2011 15:34'!startMotorPower: power 	self startMotor: ' ' power: power ! !!ScratchStageMorph methodsFor: 'scratch processes/events' stamp: 'ka 11/21/2011 16:48'!stopAll	"Stop all processes and make sure I am stepping."	| sFrame |	World hands do: [:h | h newKeyboardFocus: nil; clearUnclaimedKeystrokes].	Sensor clearKeystate.	SoundPlayer stopPlayingAll.	self class stopSoundRecorder.	self stopAllProcesses.	self stopAsks.	self deleteAllClones.	self midiAllNotesOff.	self allMotorsReset.	WeDoPlugin resetWeDo.	self stopPlaying.	self allMorphsDo: [:m |		(m isKindOf: ScriptableScratchMorph) ifTrue: [m stopPlaying]].	(sFrame _ self ownerThatIsA: ScratchFrameMorph) ifNotNil: [		sFrame scriptsPane allMorphsDo: [:m |			(m respondsTo: #stop) ifTrue: [m stop].			(m respondsTo: #litUp:) ifTrue: [m litUp: false]].		World startSteppingSubmorphsOf: sFrame].	World startSteppingSubmorphsOf: self.! !!SensorBoardMorph methodsFor: 'accessing' stamp: 'ka 2/11/2012 19:22'!command	command isNil ifTrue: [command _ 2r10000000].	^ command! !!SensorBoardMorph methodsFor: 'accessing' stamp: 'ka 12/31/2009 23:43'!command: number	command _ number! !!SensorBoardMorph methodsFor: 'private' stamp: 'ka 1/1/2010 11:54'!processIncomingData	"Process incoming bytes from the serial port."	"Details: To work around a problem with the Prolific USB-Serial drivers on some Windows machines, a strict turn-taking polling protocol is used. A poll byte is sent, the ScratchBoard sends a response, and after a small delay (to be sure that all the data from the last poll has arrived, another poll is sent. The goal is to never allow more that few bytes to accumulate in the serial input buffer and to avoid sending a poll byte while data is arriving. However, since different versions of the ScratchBoard may send different amounts of data, we don't want to hard-code the number of reply bytes. Thus"	| buf msecsSinceLastPoll byte |	(self portIsOpen and: [#on = scanState]) ifFalse: [^ self].	useGoGoProtocol ifTrue: [		buf _ port readByteArray.		buf do: [:b | self processGoGoByte: b].		^ self].	msecsSinceLastPoll _ Time millisecondClockValue - lastPollMSecs.	msecsSinceLastPoll < 20 ifTrue: [^ self].	buf _ port readByteArray.	buf do: [:b | self processScratchByte: b].	byte _ (self command min: 255) max: 0.	port nextPut: byte.	"send a ScratchBoard V4 poll byte"	lastPollMSecs _ Time millisecondClockValue.! !!SensorBoardMorph methodsFor: 'fake wedo ops' stamp: 'ka 12/27/2011 18:37'!motor: motorName direction: directionName	"Set the direction of the given motor."	| cmd |	cmd _ self command.	(directionName includesSubString: 'reverse')		ifTrue: [cmd _ cmd bitXor: 2r10000000].	(directionName includesSubString: 'this way')		ifTrue: [cmd _ cmd bitOr: 2r10000000].	(directionName includesSubString: 'that way')		ifTrue: [cmd _ cmd bitAnd: 2r01111111].	self command: cmd! !!SensorBoardMorph methodsFor: 'fake wedo ops' stamp: 'ka 12/27/2011 18:38'!motorOff: motorName	"Turn the given motor off."	| cmd |	cmd _ self command.	cmd _ cmd bitAnd: 2r10000000.	self command: cmd! !!SensorBoardMorph methodsFor: 'fake wedo ops' stamp: 'ka 12/27/2011 18:38'!motorOn: motorName 	self motorOn: motorName power: WeDoPlugin motorAPower! !!SensorBoardMorph methodsFor: 'fake wedo ops' stamp: 'ka 1/11/2012 15:47'!motorOn: motorName power: power 	| cmd |	cmd _ self command.	cmd _ cmd bitAnd: 128.	cmd _ cmd bitOr: (127 / 100 * (power abs min: 100) max: 0) rounded.	self command: cmd.	WeDoPlugin setMotorAPower: power! !!WeDoPlugin class methodsFor: 'motor control' stamp: 'ka 12/27/2011 00:07'!motorAPower: aNumber	"Set the power level to the absolute value of the given number. The range is 0-100."	self setMotorAPower: aNumber.	self updateMotors.! !!WeDoPlugin class methodsFor: 'motor control' stamp: 'ka 12/27/2011 00:07'!motorBPower: aNumber	"Set the power level to the absolute value of the given number. The range is 0-100."	self setMotorBPower: aNumber.	self updateMotors.! !!WeDoPlugin class methodsFor: 'private' stamp: 'ka 12/27/2011 00:08'!motorAPower	^ MotorAPower! !!WeDoPlugin class methodsFor: 'private' stamp: 'ka 12/27/2011 00:08'!motorBPower	^ MotorBPower! !!WeDoPlugin class methodsFor: 'private' stamp: 'ka 12/27/2011 00:06'!setMotorAPower: aNumber	"Set the power level to the absolute value of the given number. The range is 0-100."	MotorAPower _ aNumber abs! !!WeDoPlugin class methodsFor: 'private' stamp: 'ka 12/27/2011 00:06'!setMotorBPower: aNumber	"Set the power level to the absolute value of the given number. The range is 0-100."	MotorBPower _ aNumber abs! !