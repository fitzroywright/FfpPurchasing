# FFP Purchasing Tablet MVP

First working slice of the FFP Manager Purchase Order / Payment Requisition tablet application.

## Implemented
- .NET MAUI Android + iOS shell.
- Supporting-document camera capture.
- Device/local-storage multi-file picker.
- Durable app-local copy immediately after selection/capture.
- Explicit Pending Sync state for offline-first behavior.
- Android ACTION_SEND / ACTION_SEND_MULTIPLE intake so an Outlook attachment can be shared to **FFP Purchasing**.
- PDF, Office documents and image content-type handling.

## First-hour test
1. Open the app on Android.
2. Tap **Take Photo** and photograph a quotation.
3. Tap **Choose Files** and select a PDF from Downloads.
4. In Outlook for Android, open/download an attachment, choose Share, then choose **FFP Purchasing**.
5. Return to the app; the attachment should appear as **Outlook / Share • Pending Sync**.
6. Turn off network access and repeat steps 2-3. Files remain locally attached.

## Deliberately not claimed complete
- The FFP Manager purchasing API is not yet connected.
- Upload acknowledgement/retry is not yet implemented; therefore Pending Sync does not clear.
- iOS direct Outlook Share Extension is not implemented in this first slice. iOS camera and local file picking are present.
- Full PO/PR vendor/item/approval workflow remains the next vertical slice.
- Common.Diagnostics integration and discrete attachment diagnostics remain to be added.

This project intentionally contains no Aegis branding.
