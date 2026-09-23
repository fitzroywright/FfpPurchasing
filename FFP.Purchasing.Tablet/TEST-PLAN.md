# Acceptance smoke test
1. Start FFP.Purchasing.Api and point tablet Settings to its HTTPS/reachable address.
2. Create Purchase Order; enter requestor/department/vendor/purpose.
3. Add line, quantity and unit price; confirm total.
4. Take a supporting-document photo.
5. Pick PDF/Word/Excel/image from local storage.
6. Android: share an Outlook attachment to FFP Purchasing.
7. Disable network; create another request and verify local Draft/Pending Sync persists.
8. Restore network and submit; server number must be returned and attachment states become Synced.
9. Open My Requests and reopen request.
10. Run Level 5: FFP.PUR.L5.001 through .005 each logs/display individually.
11. Verify no Level 4-1 diagnostic runs as part of Level 5.
