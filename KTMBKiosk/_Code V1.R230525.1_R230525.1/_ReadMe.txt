This Version		: 
Previous Version	: Code 60 V1.R210930.1_R210930.1

Deployment Check List (After code changed)
===============================================
KTMB Latest Changes
: NetServiceAnswerMan
: RunThreadMan
: new ThreadSupervisor



Code 62 DEV.V1.R211122.1_DEV.R211122.1 Changes
: new NetServiceAnswerMan
: LocalTcpService
	- Invoke OnDataReceived without lock()
	- Use back _ecr.SendReceive. Use previous SendReceive(..) for IM20 Sale Transaction

Code 61 DEV.V1.R211112.1_DEV.R211112.1 Changes
: RPTOnlineKomuter3iNCH.rdlc
	- Enlarge 2D Barcode.
: New workflow for IM20 API sale transaction.
	- PayECRReadProtocolxSale
: Review the IM20 data transaction protocol. Send EOT when finished.
: New NssIT.Kiosk.Tools library to replace "NssIT.Kiosk.Common.ThreadMonitor"

Code 60B V1.R210930.1B_R210930.1B Changes
: Add Debug log	in IM20 API and Card Transaction UI
	- Resolve IM20 payment hang when start transaction.


xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


(Before code changed)
==================================================






