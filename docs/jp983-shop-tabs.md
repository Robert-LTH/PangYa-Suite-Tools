# JP 983 shop tabs

The Shop Editor positions the tabs at the `tab_main` and `tab_sub` origins in `shop.xml`. It uses the JP `stab_*` and `stext_*_n` textures and the separator and state backgrounds specified in the XML. A main tab selection resets the sub-tab and item page. Search applies within the selected tab.

| Main tab | `stab` image ID | `stext` image IDs in order |
| --- | ---: | --- |
| Character | 1 | |
| Clothing | 7 | 22, 2, 3, 4, 5, 27, 23 |
| Items | 2 | 7, 8, 9, 10, 28 |
| Caddie | 3 | 12 |
| Mascot | 4 | 13 |
| Decoration | 5 | 14, 15, 16, 17, 34, 35, 32 |
| My Box | 6 | 19, 20, 21, 25, 24, 38 |

Static analysis of `ProjectG_Editavel_983.exe` (SHA-256 `6a499b52236110bcad6e5a29892bca862e643e0b59ae9365b1aad71a007bc275`) found the six-entry sub-tab dispatch at VA `0x61E4E0`. The main-tab initializer at `0x6070EA` loads images 7, 2, 3, 4, 5, 6. The editor includes the requested Character tab first. The Caddie branch adds tab identifier 11 with image 12. The editor includes the branches for optional images 27, 28, 25 and 24 without checking client feature flags. The individual `st_*` textures are additional resources.

The clothing filter uses the category masks from `0xB2E230` and the JP 983 Part schema. The remaining catalog filters approximate the shop lists with IFF groups and subtype fields. My Box in the client uses player inventory. The editor has only an IFF archive, so its Coupon, Report, Replay and AF tabs show Item records for inspection. The editor includes records regardless of saleability and ownership.
