# FFP Purchasing

Tablet-first Purchase Order and Payment Requisition application for Food For The Poor Jamaica.

## Current proof target
Android is the active client proof target. iOS remains in the solution but is not a release gate yet. FFP Manager integration is deliberately behind an adapter boundary and is not a current acceptance dependency.

## Current vertical slice
- Purchase Order and Payment Requisition drafts
- Vendor/reference cache seam
- Line items, UOM, quantity, pricing and totals
- Supporting documents from camera and local storage
- Android share intake for Outlook attachments
- Offline local persistence and visible Pending Sync
- API submission and acknowledgement contract
- My Requests/history
- Level 5 non-destructive diagnostics with stable IDs
- No Aegis branding

## Next build focus
Complete Android workflow hardening, retry queue, attachment lifecycle, approval/status model, receiving/payment status, validation, audit events, diagnostics Levels 4-1, automated tests and deployment packaging. FFP Manager and iOS proof are deferred, not removed.
