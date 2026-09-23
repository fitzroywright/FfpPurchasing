# Run First Draft on the Visual Studio Laptop

## Prerequisites
Visual Studio with the .NET MAUI workload and Android SDK/emulator installed. .NET 10 SDK is required by the current projects.

## 1 - Pull
Clone or pull `fitzroywright/FfpPurchasing` and open `FfpPurchasing.slnx`.

## 2 - Start the mock server
Set `FFP.Purchasing.Api` as startup and run the **http** profile. It listens on port 5088.

Verify from the laptop browser:
`http://localhost:5088/health`

## 3 - Android emulator
Start an Android emulator. In FFP Purchasing Settings set:
`http://10.0.2.2:5088/`

Android emulator uses 10.0.2.2 to reach the Windows host.

## 4 - Run tablet
Set `FFP.Purchasing.Tablet` as startup, select the Android emulator, and Run.

## First-draft proof
- Create PO or Payment Requisition.
- Vendors come from Mock FFP Manager.
- Add a new item manually with UOM/quantity/current price.
- Create another line and observe the locally learned item/UOM behavior.
- No item endpoint or item catalog is supplied by Mock FFP Manager.
- Add photo/document.
- Submit to mock API and receive FFP document number.
- Create while offline and see Pending Sync.
- Run Level 5 diagnostics.
- iOS is not part of this milestone.

If Android blocks HTTP cleartext during emulator testing, use the local development network-security allowance added for Debug rather than weakening Release security.
