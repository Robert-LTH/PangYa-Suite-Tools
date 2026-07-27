# PangYa-Suite-Tools
Advanced PangYa File Suite Editor written in C#

[![Build Status](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://microsoft.com/windows)
[![Project Stage](https://img.shields.io/badge/Stage-In--Development-orange.svg)]()

---

## 🇺🇸 English

**Pangya Suite Tools** (or *Pangya Studio Tools*) is an integrated ecosystem for advanced reverse engineering and modification, developed entirely in **C# (.NET 10)**. This solution centralizes reading, editing, converting, and compiling multiple native file formats used by both the client and server of the game **PangYa** (such as `.PAK` structures, `.IFF` tables, and patch lists).

The project is built on top of a high-performance API (`PangyaAPI`) and a rich **Windows Forms** graphical interface, utilizing modern asynchronous Task-based operations to ensure heavy disk I/O and cryptographic tasks run smoothly in the background, keeping the UI fully responsive.

### 🗺️ Module Overview
- [x] **PangyaAPI.PAK (`FrmPakMaker.cs`)**: Surgical data package manipulation. Individual or batch extraction, dynamic file injection/merging, and full multi-region XTEA algorithm support.
- [x] **PangyaAPI.PAK Sync (`FrmPakDiff.cs`)**: Cross-client Multi-PAK structural synchronization tool to compare and isolate missing, modified, or identical files between different clients.
- [x] **PangyaAPI.IFF**: Structured parser and editor for game data tables (`Character.iff`, `Item.iff`, etc.), enabling complete customization of server attributes and item mechanics.
- [x] **PangyaAPI.UpdateList**: Utility for generating and signing encrypted XML patch lists for the game Launcher/Updater.
- [x] **PangYa UI Editor**: Opens extracted `ui/*.xml` layouts in an element tree, renders image assets and reusable frame/macro definitions referenced across XML files on a zoomable canvas, previews button states, debug bounds, and checkbox-selected `#ifdef` elements, tolerates common legacy XML defects, and atomically edits source-backed element properties. Its workflow is inspired by [Saeroun](https://github.com/retreev/Saeroun).

### 🚀 Advanced Features
- **Application Log Viewer:** A shared logging interface retains tool activity for the current session and exposes it from the main menu; PAK audit activity is also written to `activity_log.txt`.
- **Multi-Region XTEA Cryptography:** Full support for official and custom header encryptions: Global (GB), Thailand (TH), Japan (JP), Korea (KR), Indonesia (ID), Europe (EU), and Super SS Dev (Custom).
- **Advanced Tree View Interaction:** Create persistent empty folders from the right-click menu, select them as file-injection targets, and use keyboard shortcuts or context actions for rename, extraction, and subtree removal.
- **From-Scratch PAK Creation:** Use **New** in the PAK operations toolbar to configure, create, and immediately open a valid empty PAK, then build its archive layout by adding persistent folders and injecting files from the manager.

### 🛠️ Technical Snippet (PAK Compilation Example)
To code with this API to compile a folder using Japanese specification (V3):

```csharp
using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
using PangyaAPI.Utilities.Cryptography;

var writer = new PakWriter
{
    EntryVersion = PakFileEntryVersion.V3,
    EntryType = PakFileEntryType.LZ772,
    CompressLevel = 5,
    LocationKeys = PakKeys.JP,// xtea_key
    Author = "SuiteTools"
};

// Compiles recursively while preserving offsets.
writer.CreateFromDirectory(@"C:\Modding\data", @"C:\Games\PangYa\ProjectG.pak");
```

### IFF Editor / Manager

Open **IFF Editor / Manager** from the main menu to work with a folder of loose `.iff` files, a single loose IFF, or a ZIP-based PangYa IFF container. Encrypted containers are detected automatically, and the container key selector can preserve the detected XTEA key or save the archive as plain ZIP or with another supported key.

The editor provides two complementary views:

- **Form view** groups schema fields into tabs, provides record search, and uses appropriate controls for text, numbers, flags, dates, and item references. When a PangYa data folder is selected, supported references can display names and icons and open a reference picker.
- **Grid view** is intended for comparing and editing many records at once. Its columns come from the active JSON schema, while unmapped bytes remain available through the raw-record viewer.

Use the toolbar to add, copy, or delete records; save changes; extract one original IFF or every entry in a container; and patch the current table from a same-named loose IFF. The patch workflow matches records by item ID, lets you choose records and compatible fields, previews the changes, and converts values for the target region and string widths.

For the safest workflow, select **Auto** region detection first and choose the string encoding before loading a file. The editor recognizes known TH, JP, and Global headers and uses filename or container hints when available; if the region is still ambiguous, it displays the header details and asks which schema to use. Back up game data before editing. **Extract IFF** and **Extract all IFFs** export the original stored bytes, so unsaved edits are not included.

### JSON IFF schemas

IFF editor layouts are defined by versioned JSON files in
`%LocalAppData%\PangYa-Suite-Tools\schemas`. Default TH, JP, and Global schemas are copied there on first use without overwriting existing files. Schema files are matched by IFF filename and region (for example, `Item.TH.json`), with `.default.json` as the optional fallback. Version 2 schemas can inherit a `Common` base selected from the IFF header's revision and magic; version 1 flat custom schemas remain supported. The editor's **Schema columns** dialog shows inherited base fields as read-only schema definitions and saves local column changes back to the matching JSON file.
JP schemas whose records use the 192-byte `Common` base expose their format-specific fields after that base instead of treating the remaining bytes only as raw data.
Bundled schemas carry a `defaultRevision`. When a newer bundled revision differs from a local schema, the IFF Manager offers to replace the local file, keep the complete local definition as the preferred default for that revision, or defer the choice. Replaced or acknowledged files are backed up under `%LocalAppData%\PangYa-Suite-Tools\schemas\backups`; embedded defaults remain read-only and the preferred definition is always stored as a user-local override. The **Schema updates...** toolbar command reopens deferred updates.
The region selector can be set before opening an IFF; delimited `TH` or `JP` tokens in the container filename automatically update the selector, while the manual choice is used for files whose region cannot be detected.
The schema editor can clone fields from the current or another schema, reorder fields, configure a default width for new strings, and control each field's visibility. Embedded defaults are used whenever no matching user schema exists. Raw hexadecimal bytes are colored by their defining schema fields, with overlaps shown in red. The editor reports bytes whose bits are not fully represented by schema fields.
Double-click a Raw grid cell to open its byte-range picker. Select one contiguous range and choose **Define column** to create a schema field prefilled with the selected offset and width.
ZIP-based IFF containers expose their detected XTEA key and can be saved as plain ZIP or with another supported key. Delimited `TH`/`JP` filename tokens select the schema region, while encryption keys are detected independently by trying the supported keys.
The editor detects the IFF region from known header revision and magic combinations, or from an explicit filename/container region hint when necessary. Only when none of those sources resolves a region does it display the record count, revision, magic, reserved bytes, and record size and require a TH, JP, or Global schema choice. The form-view search includes the full `Description` text in `Desc.iff`. The **Extract IFF** action exports the selected entry's original bytes without applying unsaved editor changes. **Extract all IFFs** exports every archive entry into one selected folder, flattening internal paths and replacing existing files atomically after checking for filename collisions. The **Patch IFF** action copies compatible values from a same-named loose IFF by matching item IDs, previewing selected records, and converting values through the target region's schema and string widths.

#### Column-header scrolling

Hover an IFF grid column header and use the mouse wheel to adjust its schema field. Wheel up increases the value by one byte; wheel down decreases it.

- **Ctrl + wheel:** adjust the field offset and move following fields with it.
- **Alt + wheel:** adjust the field width and update following offsets.
- Hold **Shift** with either shortcut to change only the hovered field, leaving following fields unchanged.

Valid changes are saved immediately to the matching user JSON schema. Invalid changes, such as moving a field outside the fixed record size, are rejected.

---

## 🇧🇷 Português

**Pangya Suite Tools** (ou *Pangya Studio Tools*) é uma solução integrada de engenharia reversa e modificação avançada, desenvolvida inteiramente em **C# (.NET 10)**. O objetivo deste ecossistema é centralizar a leitura, edição, conversão e compilação de múltiplos formatos de arquivos nativos utilizados pelo cliente e servidor do jogo **PangYa** (como estruturas `.PAK`, tabelas `.IFF` e listas de atualização).

O projeto é estruturado sobre uma API de alto desempenho (`PangyaAPI`) e uma interface gráfica rica em **Windows Forms**, utilizando operações assíncronas modernas baseadas em Tasks para garantir que tarefas pesadas de I/O de disco e criptografia ocorram em background, mantendo a UI totalmente responsiva.

### 🗺️ Visão Geral dos Módulos
- [x] **PangyaAPI.PAK (`FrmPakMaker.cs`)**: Manipulação cirúrgica de pacotes de dados. Extração individual ou em lote, injeção/mesclagem dinâmica de arquivos e suporte total ao algoritmo XTEA multiregião.
- [x] **PangyaAPI.PAK Sync (`FrmPakDiff.cs`)**: Ferramenta de sincronização estrutural Multi-PAK entre clientes para comparar e isolar arquivos ausentes, modificados ou idênticos.
- [x] **PangyaAPI.IFF**: Parser e editor estruturado para tabelas de dados do jogo (`Character.iff`, `Item.iff`, etc.), permitindo a customização completa de atributos, itens e mecânicas internas do servidor.
- [x] **PangyaAPI.UpdateList**: Utilitário para geração e assinatura de listas criptografadas em XML para o Launcher/Updater do jogo.
- [x] **Editor de UI PangYa**: Abre layouts `ui/*.xml` extraídos em uma árvore de elementos, renderiza os recursos em uma tela com zoom, visualiza estados de botões e limites de depuração e edita propriedades de forma atômica. O fluxo é inspirado no [Saeroun](https://github.com/retreev/Saeroun).

### 🚀 Recursos Avançados
- **Visualizador de Log do Aplicativo:** Uma interface de log compartilhada mantém a atividade das ferramentas durante a sessão e pode ser aberta pelo menu principal; a auditoria de PAK também é gravada em `activity_log.txt`.
- **Criptografia por Região (XTEA):** Suporte completo ao algoritmo XTEA para criptografia de cabeçalhos utilizando chaves oficiais e customizadas: Global (GB), Tailândia (TH), Japão (JP), Coreia (KR), Indonésia (ID), Europa (EU) e Super SS Dev (Custom).
- **Interação Avançada em Árvore:** Crie pastas vazias persistentes pelo menu de contexto, selecione-as como destino para injeção de arquivos e use atalhos ou ações de contexto para renomear, extrair e remover subárvores.
- **Criação de PAK do Zero:** Crie e abra imediatamente um PAK vazio válido e monte sua estrutura adicionando pastas persistentes e injetando arquivos pelo gerenciador.

### Editor / Gerenciador de IFF

Abra o **Editor / Gerenciador de IFF** pelo menu principal para trabalhar com uma pasta de arquivos `.iff` soltos, um único IFF ou um contêiner IFF do PangYa baseado em ZIP. Contêineres criptografados são detectados automaticamente, e o seletor de chave permite manter a chave XTEA detectada ou salvar o arquivo como ZIP simples ou com outra chave compatível.

O editor oferece duas visualizações complementares:

- **Visualização em formulário:** organiza os campos do esquema em abas, permite pesquisar registros e usa controles apropriados para textos, números, flags, datas e referências de itens. Ao selecionar uma pasta de dados do PangYa, as referências compatíveis podem exibir nomes e ícones e abrir um seletor de itens.
- **Visualização em grade:** facilita a comparação e edição de vários registros. As colunas são definidas pelo esquema JSON ativo, e os bytes não mapeados continuam acessíveis pelo visualizador de registro bruto.

Use a barra de ferramentas para adicionar, copiar ou excluir registros; salvar alterações; extrair o IFF original selecionado ou todas as entradas do contêiner; e aplicar um patch a partir de um IFF solto com o mesmo nome. O assistente de patch relaciona registros pelo ID do item, permite selecionar registros e campos compatíveis, mostra uma prévia e converte os valores para a região e as larguras de texto do arquivo de destino.

Para um fluxo mais seguro, mantenha a detecção de região em **Automático** e escolha a codificação de texto antes de carregar o arquivo. O editor reconhece cabeçalhos conhecidos das regiões TH, JP e Global e usa dicas do nome do arquivo ou contêiner quando disponíveis; se a região continuar ambígua, ele exibe os detalhes do cabeçalho e solicita o esquema correto. Faça backup dos dados do jogo antes de editar. **Extrair IFF** e **Extrair todos os IFFs** exportam os bytes originais armazenados, portanto alterações ainda não salvas não são incluídas.

### Rolagem nos cabeçalhos das colunas IFF

Posicione o cursor sobre o cabeçalho de uma coluna da grade IFF e use a roda do mouse para ajustar o campo do esquema. Rolar para cima aumenta o valor em um byte; rolar para baixo diminui.

- **Ctrl + roda:** ajusta o deslocamento do campo e move os campos seguintes.
- **Alt + roda:** ajusta a largura do campo e atualiza os deslocamentos seguintes.
- Segure **Shift** com qualquer atalho para alterar somente o campo sob o cursor, sem modificar os campos seguintes.

Alterações válidas são salvas imediatamente no esquema JSON do usuário. Alterações inválidas, como mover um campo para fora do tamanho fixo do registro, são rejeitadas.

### 🛠️ Trecho Técnico (Exemplo de Compilação PAK)
Código base para compilar uma pasta de modificações usando a especificação do cliente Japonês (V3):

 ```csharp

using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
using PangyaAPI.Utilities.Cryptography;

var writer = new PakWriter
{
    EntryVersion = PakFileEntryVersion.V3,
    EntryType = PakFileEntryType.LZ772,
    CompressLevel = 5,
    LocationKeys = PakKeys.JP, // xtea_key
    Author = "SuiteTools"

};
// Compila recursivamente mantendo a integridade dos offsets
writer.CreateFromDirectory(@"C:\Modding\data", @"C:\Games\PangYa\ProjectG.pak"); ```

```
