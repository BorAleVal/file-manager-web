const rootPath = "C:\\";
var currentPath = rootPath;
const jsonData = [
  { ID: "001", HD_Name: "C:\\" },
  { ID: "002", HD_Name: "D:\\" },
];

const selectElement = document.getElementById("selectHD");
const currentPathElement = document.getElementById("currentPath");
currentPath.textContent = currentPath;
fillFileManager(rootPath);

jsonData.forEach((data) => {
  const option = document.createElement("option");
  option.value = data.ID;
  option.textContent = data.HD_Name;
  selectElement.appendChild(option);
});

const json =
  '[{"name":"John sa ads asdfasdsd sad s dds fdsafds", "size":30, "modifiedOn":"25.04.17"}, {"name":"Jane", "size":27, "modifiedOn":"24.04.17"}]';
const data = JSON.parse(json);
const table = document.getElementById("folder-content");
const content = document.getElementById("folder-content-list");

function fillTable(data) {
  while (content.childNodes.length) {
    content.removeChild(content.childNodes[0]);
  }

  // Заполнение таблицы данными
  data.forEach((item) => {
    const row = table.insertRow();

    Object.entries(item).forEach(([key, value]) => {
      const cell = row.insertCell();
      cell.textContent = value;
      if (key == "name") {
        cell.addEventListener("click", function () {
          onContentItemClick(cell);
        });
        cell.style.cursor = "pointer";
        cell.innerHTML = '<i class="fa-solid fa-file"></i>' + cell.innerHTML;
      } else {
        cell.style.textAlign = "right";
        cell.style.width = "60px";
      }
    });
  });
}

document.body.appendChild(table);

function onContentItemClick(btn) {
  console.log("fillFileManagerBtn");
  let path = btn.textContent;
  if (path == undefined) return;
  currentPath = path;
  fillFileManager(path);
}

function fillFileManager(path) {
  fetch(`/api/SystemInfo/pathcontent?path=${encodeURIComponent(path)}`)
    .then((response) => response.json())
    .then((data) => {
      fillTable(data);
    })
    .catch((error) => {
      console.log(error);
    });
  console.log("goTo function invoked");
}

function onButtonUpLevelClick() {
  fetch(`/api/SystemInfo/ownerPath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((text) => {
      currentPath = text;
      fillFileManager(currentPath);
    })
    .catch((error) => {
      console.log(error);
    });
}
