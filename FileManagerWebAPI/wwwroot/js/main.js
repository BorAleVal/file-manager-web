const rootPath = "C:\\";
var currentPath = rootPath;
const iconMap = new Map([
  ["folder", "folder"],
  [".xlsx", "file-excel"],
  [".pdf", "file-pdf"],
]);

const jsonData = [
  { ID: "001", HD_Name: "C:\\" },
  { ID: "002", HD_Name: "D:\\" },
];

const selectElement = document.getElementById("selectHD");
const currentPathElement = document.getElementById("currentPath");
currentPathElement.innerText = currentPath;
fillFileManager(rootPath);

jsonData.forEach((data) => {
  const option = document.createElement("option");
  option.value = data.ID;
  option.innerText = data.HD_Name;
  selectElement.appendChild(option);
});

const table = document.getElementById("folder-content");
const content = document.getElementById("folder-content-list");

function fillTable(data) {
  while (content.childNodes.length) {
    content.removeChild(content.childNodes[0]);
  }

  // Заполнение таблицы данными
  data.forEach((item) => {
    const row = content.insertRow();

    Object.entries(item).forEach(([key, value]) => {
      if (key == "type") return;

      const cell = row.insertCell();
      cell.innerText = value;
      if (key == "name") {
        cell.addEventListener("click", function () {
          onContentItemClick(cell);
        });
        cell.style.cursor = "pointer";
        let iconName = "file";
        if (iconMap.has(item.type)) {
          iconName = iconMap.get(item.type);
        }

        cell.innerHTML = `<i class="fa fa-${iconName}"></i>` + cell.innerHTML;
      } else {
        cell.style.textAlign = "right";
        cell.style.width = "60px";
      }
    });
  });
}

function onContentItemClick(btn) {
  console.log("fillFileManagerBtn");
  let path = btn.innerText;
  if (path == undefined) return;
  currentPath += currentPath.endsWith("\\") ? path : "\\" + path;
  currentPathElement.innerText = currentPath;
  fillFileManager(path);
}

function fillFileManager() {
  fetch(`/api/SystemInfo/pathcontent?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.json())
    .then((data) => {
      fillTable(data);
    })
    .catch((error) => {
      console.log(error);
    });
}

function onButtonUpLevelClick() {
  fetch(`/api/SystemInfo/ownerPath?path=${encodeURIComponent(currentPath)}`)
    .then((response) => response.text())
    .then((text) => {
      currentPath = text;
      currentPathElement.innerText = currentPath;
      fillFileManager(currentPath);
    })
    .catch((error) => {
      console.log(error);
    });
}
