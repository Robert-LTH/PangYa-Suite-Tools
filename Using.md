# 📘 PangYa Suite Tools — Full Documentation

> Official usage guide for **PangYa Suite Tools**, the integrated C# (.NET WinForms) toolset for reading, editing, packaging, and synchronizing **PangYa** game files (`.PAK`, `.IFF`, `.WFT`, and `updatelist`).

This documentation is written for **end users** — modders, private server administrators, and developers — who are just getting started with the tool or running into specific problems, such as `.IFF` corruption on save or `.PAK` packages that fail to load in the client.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Interface Guide and Full Form Mapping](#2-interface-guide-and-full-form-mapping)
   - [2.1 FrmMenu — Main Menu](#21-frmmenu--main-menu)
   - [2.2 FrmIFFManager — .IFF File Manager](#22-frmiffmanager--iff-file-manager)
   - [2.3 FrmPakMaker — .PAK Creator and Packager](#23-frmpakmaker--pak-creator-and-packager)
   - [2.4 FrmPakDiff — Package Comparator / Diff Tool](#24-frmpakdiff--package-comparator--diff-tool)
   - [2.5 FrmUpdateList — Updatelist Manager](#25-frmupdatelist--updatelist-manager)
   - [2.6 FrmWftViewer — WFT File Viewer](#26-frmwftviewer--wft-file-viewer)
   - [2.7 FrmOptions — Options / Settings](#27-frmoptions--options--settings)
   - [2.8 FrmLog — Log Window](#28-frmlog--log-window)
   - [2.9 IffFieldTemplateDialog — Field Template Dialog](#29-ifffieldtemplatedialog--field-template-dialog)
3. [Recommended Workflow](#3-recommended-workflow-step-by-step)

---

## 1. Project Overview

**PangYa Suite Tools** (also called *PangYa Studio Suite*) is a single solution that centralizes the core modding and maintenance tasks for a PangYa client/server:

- Editing the game's data tables (`Character.iff`, `Item.iff`, `Desc.iff`, etc.);
- Creating, extracting, and rebuilding `.PAK` packages (with multi-region XTEA support);
- Structurally comparing versions of `.PAK` files or entirely different clients;
- Generating and signing the update list (`updatelist`) used by the Launcher/Updater;
- Viewing bitmap `.WFT` fonts;
- Experimental editing of UI layouts (`ui/*.xml`).

The whole application is built on top of a proprietary high-performance API (`PangyaAPI`), with asynchronous (`async`/`Task`) operations so that heavy disk I/O and cryptographic routines never freeze the interface.

> **WARNING:** PangYa Suite Tools **edits and rebuilds original binary game files**. Mistakes can corrupt `.IFF` and `.PAK` files. Read section 4 (FAQ) carefully and always keep backups before editing production files.

### System requirements and prerequisites

| Item | Requirement |
|---|---|
| Operating system | Windows (native WinForms application) |
| Runtime | .NET 10 (the published build is self-contained, so a separate .NET install is not required) |
| Architecture | x64 |
| Permissions | Normal execution for most tasks; **"Run as Administrator"** is only needed to register file associations/context menu entries in Windows (Options screen) |
| Disk space | At least 2x the size of the `.PAK`/`.IFF` files being edited is recommended, since the tool automatically creates backup copies (`.bak`) during rebuilds |

### General navigation structure

The program follows a **central hub** model: on launch, the user lands on **FrmMenu** (Main Menu), from which each module opens in its own independent window. Opening most modules hides the main menu, which reappears automatically once the child tool is closed — so the workflow is always: **Menu → Tool → Close → Menu**.

```
FrmMenu (Main Menu)
 ├── 📦 PAK File Manager                 → FrmPakMaker
 ├── 🌐 Patch / UpdateList Manager       → FrmUpdateList
 ├── 📝 IFF Editor / Manager             → FrmIFFManager
 ├── 🔍 PAK Comparator (Diff)            → FrmPakDiff
 ├── Options                             → FrmOptions
 ├── View log                            → FrmLog
 ├── Shop/UI Editor                      → FrmPangyaUiEditor
 └── WFT Font Viewer                     → FrmWftViewer
```

Language settings are global and apply in real time to every open module (Portuguese, English, Swedish, Japanese, and French are supported).

---

## 2. Interface Guide and Full Form Mapping

### 2.1 FrmMenu — Main Menu

**Purpose:** the application's single entry point. Each button opens a specific module in a new window.

**How to use it:**
1. On startup, **FrmMenu** is displayed.
2. Click the button for the module you need (PakMaker, UpdateList, IFF Manager, PakDiff, Options, Log, Shop/UI Editor, or WFT Font Viewer).
3. Clicking one of the main modules (PakMaker, UpdateList, IFF Manager, PakDiff, Shop/UI, WFT Fonts) automatically hides the menu until you close that tool's window.
4. **Options** and **Log** are exceptions: they open *without* hiding the menu, so you can consult them alongside another open tool.

**Common issues:**
- **"The Shop/UI Editor button freezes for a few seconds when clicked"** — this is expected: the editor asks you to select the game's extracted data folder (`FolderBrowserDialog`) and loads the UI XML files asynchronously before opening the window.
- **"The menu doesn't reappear after I closed the tool"** — this usually indicates the child tool crashed before properly disposing itself. Close the process from Task Manager and reopen the program; check the **Log** window (section 2.8) for error details before reporting a bug.

---

### 2.2 FrmIFFManager — .IFF File Manager

**Purpose:** the most critical module in the suite. It lets you open, inspect, edit, and save the game's internal data tables (`Character.iff`, `Item.iff`, `Desc.iff`, and others), whether as loose `.iff` files in a folder or as a compressed/encrypted `.iff` container (PangYa's internal ZIP-based format, with XTEA key support).

#### How to load .IFF files

1. Click **Browse...** next to **IFF Directory (Folder)** and select the extracted folder containing your `.iff` files (e.g., `Character.iff`, `Item.iff`).
2. The program scans the directory and lists every file with a `.iff` extension found under **Detected IFF Files**. The status bar shows "Scan complete — X .iff file(s) found".
3. Alternatively, use the **Open archive** toolbar button to load a single (compressed) `.iff` container directly, without pointing to a whole folder.
4. Before loading, adjust (if needed) the header region (**Auto**, Thailand, Japan, or Global) and the container key in the toolbar selectors — the tool auto-detects the header type in most cases, but this lets you force the correct schema when a file is ambiguous.
5. Double-click (or select) the desired file in the list to load its record table into the editing panel.

> **IMPORTANT STEP:** Select the **Auto** region and confirm the text encoding **before** loading the file. The editor automatically recognizes known headers (TH, JP, Global) by their binary signature and by hints in the file/container name. Only when the region remains ambiguous does the tool ask which schema to use — picking the wrong schema at that point is the most common cause of fields displaying nonsensical values.

#### How to view, change, and edit internal fields

The editor offers two complementary views, toggled via the **Form View** and **Grid View** toolbar buttons:

- **Form View:** organizes schema fields into tabs, uses appropriate controls for each data type (text, numbers, flags, dates, item references), and lets you search records by name/description. When a PangYa data folder is selected, reference fields display the item's name and icon and open a dedicated picker.
- **Grid View:** ideal for comparing and editing many records at once. Columns come from the active JSON schema; bytes with no mapped column remain accessible through the **Raw Record** viewer.

Use the toolbar to:
- **Add row / Delete rows** — creates or removes records from the loaded table (limit of 65,535 rows per `.iff` file);
- **Manage columns** — opens the schema editor, where you can clone fields from another schema (via **IffFieldTemplateDialog**, section 2.9), reorder fields, and set the default width for new strings;
- **Schema updates** — checks whether a newer revision of the default schema is available and offers to replace it, keep the local definition, or defer the decision;
- **Patch** — applies values from a same-named loose `.iff` onto the loaded table, matching records by **item ID**.

**Fine-tuning via mouse wheel (column header):** hover over a grid column header and use the mouse wheel to adjust the schema field (one byte per wheel "click"):
- **Ctrl + wheel:** adjusts the field's *offset* and shifts the following fields along with it;
- **Alt + wheel:** adjusts the field's *width* and updates the following offsets;
- **Shift + (Ctrl or Alt) + wheel:** changes **only** the field under the cursor, leaving the following fields untouched.

Valid changes made this way are saved immediately to the user's JSON schema; changes that would push a field outside the record's fixed size are automatically rejected.

#### 🔒 Critical Guide: how to SAVE/EXPORT without corrupting the .IFF structure

> **WARNING — READ BEFORE SAVING:** `.IFF` is a binary format with a **fixed record size**. Any inconsistency between the schema (columns) and the actual record size can produce a corrupted file that's unreadable by the game.

Follow this sequence every time you save:

1. **Confirm the correct region/schema** before editing (see above). Switching schemas after you've already edited values can reinterpret the bytes incorrectly.
2. Edit the required fields via **Form View** (safer for pinpoint edits) or the **Grid** (for bulk editing).
3. If any field shows an "unrepresented bytes" warning, that means part of the record has no mapped column — this is not an error, but it indicates that region of the record can only be edited through the **Raw Record** viewer.
4. Click **Save**. The program:
   - checks whether there are pending changes (if not, nothing is rewritten);
   - asks for **overwrite confirmation** before writing over the original file;
   - writes the new structure while preserving the record size defined by the schema.
5. Wait for the **"Saved"** status message before closing the program or switching files.

> **IMPORTANT STEP — Difference between Save and Extract:** the **Extract IFF** and **Extract all IFFs** buttons export the **original bytes stored in the container**, meaning **unsaved changes are not included** in the extraction. Always **Save** first if you want your edits persisted; use **Extract** only to obtain a copy of content that has already been written.

**Tips to avoid data loss:**
- Manually back up the original `.iff` folder/file before any editing session — the tool already creates automatic backups in several flows (schema patching, key changes, PakMaker removals), but the `.iff` itself is overwritten when you click **Save**, subject to confirmation.
- When using the **Patch IFF** feature, carefully review the summary shown before confirming — the operation copies values from a loose `.iff` by **item ID**, automatically converting to the target file's region and string width.
- Never interrupt (close the window, shut down the PC) during a batch extraction ("Extract all IFFs") — the tool checks for filename collisions and replaces existing files atomically, but an abrupt shutdown mid-process can leave the destination folder incomplete.
- If the schema warning (`SchemaWarning`) appears when loading a file, treat it as an alert that the schema may be outdated for that revision — review it under **Schema updates** before editing.

---

### 2.3 FrmPakMaker — .PAK Creator and Packager

**Purpose:** the surgical `.PAK` manipulation module — the compressed archive format (with multi-region XTEA encryption support) used by the PangYa client to distribute assets and data.

#### What the module is and how to create new .PAK packages

- Use **File → New** (or the equivalent button in the **PAK Operations** toolbar) to configure (entry version, compression type, compression level, region/author key) and immediately create a valid, empty `.PAK`, already opened in the manager.
- To open an existing `.PAK`, use **File → Open** or simply **drag and drop** the `.pak` file onto the window (the tool validates the extension and warns if the dropped file isn't valid).
- The package header shows **Author**, **Version**, and **Entries** (file count) as soon as it loads.

#### How to add files and folders to the packaging list

1. With an active `.PAK` loaded, use the side tree to navigate through the package's internal folders.
2. Right-click to access the context menu:
   - **📁 New folder...** creates a persistent empty folder inside the PAK structure (useful for preparing the hierarchy before injecting files);
   - **✏️ Rename Folder/File (F2)** renames the selected item;
   - Use the **inject/update files** option to pick, in the file picker, the files to add (the default filter covers `.iff`, `.tga`, `.png`, `.jpg`, `.jpeg`, `.bmp`, `.dds`, `.wav`, `.mp3`, `.txt`, `.ini`, `.xml`, `.dat`, plus "All files").
3. Once confirmed, the tool merges the new files with the existing content and **rebuilds the package automatically**.

#### How to generate and save the final, compatible .PAK file

- Every operation that changes the structure (injection, removal, folder creation, renaming) **rebuilds the `.PAK` on disk automatically** — there is no separate "Save" button for these changes, since each action already writes the final result. A `.bak` backup of the previous file is created before rebuilding.
- To generate a copy compatible with another game region, use the key change (target region/XTEA algorithm) in the PAK operations menu — the tool asks for confirmation before rebuilding and creating the `.bak`.
- Use **Extract selected...** or **Extract this folder...** to export specific files/folders, or batch extraction of multiple `.pak` files from one folder to another (bulk extraction).

**Common issues:**
- **"No valid file or folder was found for injection"** — the selection made in the dialog contained no readable files; check the path and try again.
- **"Please drag a valid .pak extension file to open"** — you dragged a file of another type (e.g., `.zip`) directly onto the window.
- **"The PAK is already using that key"** — an attempt to switch to the same key/region that's already active; no action needed.
- When **removing** files or folders, the tool always asks before rebuilding the `.PAK` and creates the `.bak` backup — never shut down the program during that rebuild.

> **WARNING:** an empty **Author** field triggers a confirmation prompt to auto-reset the author name to "PakMaker". If you want to preserve the package's original authorship, cancel that reset.

---

### 2.4 FrmPakDiff — Package Comparator / Diff Tool

**Purpose:** identify what changed between two versions of the game's data, either by comparing **snapshots of `.PAK` folders** over time, or by comparing **two full clients** to extract only the diverging files.

The module is split into two tabs:

**"📋 Change History / Log" tab (Snapshots A/B):**
1. Under **📸 Snapshot A — Before/Base**, provide the PAK folder path and click to take a snapshot (or load a previously saved `.paksnap` snapshot).
2. Repeat the process under **📸 Snapshot B — After/New** with the latest version of the same folder.
3. Click **Compare** to generate the list of **ADDED**, **REMOVED**, and **MODIFIED** files between the two snapshots.
4. Use **💾 Save log .txt** to export the comparison report.
5. Snapshots can be saved (`.paksnap`, JSON format) and reloaded later, letting you compare against older builds without needing the original files on hand.

**"🔍 Compare clients and extract" tab:**
1. Provide the **base client** folder path (reference) and the **target client** folder path (the one you want to audit).
2. Click compare — the tool reads both directories and classifies each file as **New**, **Modified**, or **Identical**.
3. Check the desired items in the results list and use **📦 Extract selected** to copy only the diverging files to a destination folder — useful for preparing incremental update packages.

**Common issues:**
- **"Select a valid folder" / "Select valid directories for both clients"** — one of the two path fields is empty or points to a nonexistent folder.
- **"Snapshot [A/B] has not been taken yet"** — attempting to compare before generating (or loading) both snapshots.
- **"No log to save. Run a comparison first"** — the save-log button was clicked before any comparison was run.

---

### 2.5 FrmUpdateList — Updatelist Manager

**Purpose:** generate, view, and sign (encrypt) the XML update list consumed by the game's Launcher/Updater, as well as monitor a client folder in real time to detect changes.

#### How to generate and update the update list

1. Select the **PangYa root folder** (where the executables and `.pak` files live) as the source.
2. Select the **destination WebServer folder** for the update (where the final `updatelist` and packages will be published).
3. Click **Generate now** to produce the list based on the current state of the source folder, or click **▶️ Start Monitoring** to have the tool watch the folder (via `FileSystemWatcher`) and regenerate automatically whenever a file changes. The status switches from **INACTIVE** to **ACTIVELY MONITORING** while the service is running; use **🛑 Stop Monitoring** to turn it off.

#### How to add new entries, paths, and validation hashes

- Every auto-generated entry includes: **File**, **Folder**, **Size**, **CRC**, **Date**, **Time**, plus **Package** and **Package size** metadata when the file belongs to a `.pak`.
- The status bar summarizes processing: `Patch: {name} | Patch number: {n} | UpdateList: {path} | Files: {processed}/{total}`.
- To inspect or load an **existing/encrypted** `updatelist`, use **Select existing encrypted updatelist (optional)**, or simply drag the file onto the indicated area ("🪂 Drag and drop an encrypted 'updatelist' file here to view the decoded XML in real time"). The tool automatically tries the known keys (brute-force scan against the key database) and reports **SUCCESS** with the identified key, or **TOTAL FAILURE** if no key in the database can decode the file.

#### Structure and correct syntax of the generated file

- The **Raw XML** tab shows the decoded content of the `updatelist`, useful for manual review before publishing.
- Once generation/signing completes, the tool reports **"updatelist signed successfully!"** along with the trigger used.

**Common issues:**
- **"Check whether the Source and WebServer Destination folders are valid directory paths"** — one of the two configured paths doesn't exist or is inaccessible.
- **"⚠️ Invalid or corrupted file" / "❌ Critical failure while parsing file"** — the dragged `updatelist` is not a file recognized by the tool, or it's truncated.
- **"❌ Error: No key decoded the structure"** — the file uses a custom encryption key not present in the known key database; this isn't a bug, it's an expected limitation for private keys used by custom servers.
- **"Could not manage the file [...]" (I/O Error)** — usually indicates the file is open/locked by another process (e.g., the Launcher itself running) or the folder lacks write permission.

> **WARNING:** keeping **Monitoring** active while you manually edit files in the watched folder can trigger multiple regenerations in a row. For large batch edits, prefer turning monitoring off, editing everything, and then clicking **Generate now** once.

---

### 2.6 FrmWftViewer — WFT File Viewer

**Purpose:** the `.WFT` format stores the **bitmap fonts (`WFNT`)** used by PangYa's interface. This viewer lets you inspect a font's glyphs without loading the entire file into memory.

#### How to load, view data/structures, and inspect .WFT content

1. Click **Open WFT** (or drag a `.wft` file onto the window) and select the desired font. Only files with a `.wft` extension compatible with PangYa are accepted.
2. After loading, the metadata bar shows: cell size in pixels, coverage in bits, total glyph count, and the header's reserved value.
3. Use the virtualized grid to browse the font's entire BMP (Basic Multilingual Plane) glyph range.
4. Type a value into **Code point:** (hexadecimal format, from `U+0020` to `U+FFFF`) and click **Go** to jump directly to a specific glyph.
5. Select a glyph to see its **Character**, **U+XXXX** code, and individual **Advance** (width, in pixels).
6. Type text into the **Sample text** box and adjust the **Sample zoom** to preview how the font renders a full phrase.

**Common issues:**
- **"Only PangYa .wft font files are supported"** — a file with a different extension was selected/dropped.
- **"Enter a hexadecimal code point compatible with the loaded font"** — the value typed in "Code point" is outside the range supported by the current font, or isn't a valid hexadecimal.
- **"Failed to open the WFT font: [detail]"** — the file is corrupted or doesn't follow the expected `WFNT` structure.

---

### 2.7 FrmOptions — Options / Settings

**Purpose:** global system adjustments — interface language and the tool's integration with Windows Explorer.

**Available settings:**
- **Language:** selector with Portuguese (BR), English (US), Swedish, Japanese, and French. The switch is applied **immediately** across every open window, with no restart required.
- **Register SuiteTools to open .pak files:** associates the `.pak` extension with PangYa Suite Tools in the current user's registry (`HKCU\Software\Classes\.pak`), allowing packages to be opened with a double-click in Explorer.
- **Add SuiteTools shell context to Windows Explorer:** adds an **"Open with PakMaker"** option to the right-click context menu of any file in Explorer.

> **WARNING:** the screen itself warns — *"SuiteTools must be started 'As Administrator' to modify shell entries!"*. Although the keys used live under `HKEY_CURRENT_USER` (which normally doesn't require administrator rights on most Windows setups), on machines with restrictive group policies the write can fail silently without elevating the process. If the checkboxes don't persist after clicking **OK**, close the program, run it as administrator, and repeat the process.

Clicking **OK** writes the changes to the registry and notifies Windows Explorer to refresh the associated icons. **Cancel** discards any changes made on the screen.

**Common issue:**
- **"Failed to apply changes: [message]"** — usually caused by insufficient registry write permission; try running as administrator.

---

### 2.8 FrmLog — Log Window

**Purpose:** consolidate the activity history of every module used during the current session into a single window — essential for diagnosing errors before reporting a problem.

**How to use it:**
1. Open it via the **View log** button in the main menu. If the window is already open, clicking again simply brings it to the front (or restores it, if minimized) instead of opening a second instance.
2. The list shows log messages with a severity level (information, warning, error) generated by any tool used during the session — opening files, saving, validation errors, PAK operation results, and so on.
3. Use **Clear view** to empty the list shown on screen (this does not delete history already written to disk).
4. Check **Log to file** to also persist messages to the `activity_log.txt` file, useful for reviewing after closing the program or for attaching when reporting a bug.

**How to interpret log messages, processing errors, and save confirmations:**
- **Error** messages usually correspond to a `MessageBox` that also appeared on screen at the moment of failure — the log window is there to review the full history after the fact.
- Success confirmations (e.g., "Saved", "PAK updated successfully!", "updatelist signed successfully!") indicate the operation completed and was written to disk.
- If an error appears without a clear on-screen message (for example, during batch operations), check the **Log** first before repeating the action.

---

### 2.9 IffFieldTemplateDialog — Field Template Dialog

**Purpose:** speed up the creation of new columns in the **IFF Manager** by reusing field definitions that already exist in other compatible schemas, instead of building a field from scratch.

**How to apply, load, or create field templates:**
1. From the **Manage columns** dialog in the IFF Manager, choose the option to select a field via template.
2. In the dialog, pick the **Source schema** from the top combo box — only schemas whose `.iff` shares a record size compatible with the current file are listed.
3. The list below shows that schema's available fields, formatted as `Name — Type, Offset, Width byte(s)`.
4. Double-click a field (or select it and click **Select**) to apply it as the basis for the new column in the current file.
5. Click **Cancel** to close without applying any template.

> **Technical note:** the dialog only lists fields whose *offset + width* fits inside the record size (`recordSize`) of the currently loaded `.iff` file — templates from an `.iff` with a very different layout (e.g., `Character.iff` vs. `Item.iff`) will normally not show up as compatible, which is expected and prevents the creation of invalid columns.

---

## 3. Recommended Workflow (Step by Step)

A complete, safe modding cycle from start to finish:

1. **Backup:** copy the original data folder (loose `.iff` files or container) and the `.pak` files you're going to change to a safe location outside the working folder.
2. **Open/Edit `.IFF`** in **FrmIFFManager** (section 2.2): load the folder/container, confirm the region and schema, edit the required records (Form View for pinpoint edits, Grid View for bulk editing), and **Save**, confirming the overwrite.
3. **Test in FrmWftViewer** (section 2.6), if the changes involve text/UI: check whether the fonts used on the affected screens correctly render the strings you added/changed.
4. **Compare via FrmPakDiff** (section 2.4): take a "Before" snapshot before touching the destination `.pak` files and an "After" snapshot at the end, to generate an audit log of everything that was added, removed, or modified — or compare directly against the reference client to make sure only the intended files changed.
5. **Package via FrmPakMaker** (section 2.3): inject the edited `.iff` files (and other assets) into the destination `.pak`(s). The tool rebuilds the package automatically and creates a safety `.bak`.
6. **Update in FrmUpdateList** (section 2.5): point the source at the updated client folder and the destination at the WebServer folder, generate (or let monitoring generate) the new signed `updatelist`, ready for distribution through the Launcher.

```
Backup → IFF Manager (edit + save) → WFT Viewer (validate fonts)
       → PAK Diff (audit changes) → PAK Maker (package)
       → UpdateList (generate/sign) → Publish
```

---

*Documentation generated for PangYa Suite Tools — covers FrmMenu, FrmIFFManager, FrmPakMaker, FrmPakDiff, FrmUpdateList, FrmWftViewer, FrmOptions, FrmLog, and IffFieldTemplateDialog.*
