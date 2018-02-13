# Forge Abstract Server Auth

An attempt at making a viable, modular approach of server auth using Forge Networking.

Issues:

- Client sees a copy of itself + of its NetworkInputController (not seen on server).
- Inactive entities (not controlled by NIC) aren't synced - normal, not coded.
- Clients can't move.
- Clients get reconciliated every single frame because localStatusHistory never contains a NetEntityStatus with the same frame as the server's for some reason.
