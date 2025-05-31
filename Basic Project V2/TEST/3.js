let btnCurrent = document.querySelector(".curr-courses");
let btnArchive = document.querySelector(".archive-courses");

btnArchive.addEventListener("click", function () {
  let currentCards = document.querySelectorAll(".course-list");
  let archiveCards = document.querySelectorAll(".course-archive");

  currentCards.forEach((card) => {
    card.style.display = "none";
  });

  archiveCards.forEach((card) => {
    card.style.display = "flex";
  });

  btnArchive.style.backgroundColor = "#e9e9e9";
  btnCurrent.style.backgroundColor = "#D9D9D9";
  btnArchive.style.position = "relative";
  btnArchive.style.left = "9rem";
  btnCurrent.style.position = "absolute";

  btnJoin.style.display = "none";
});

btnCurrent.addEventListener("click", function () {
  let archiveCards = document.querySelectorAll(".course-archive");
  let currentCards = document.querySelectorAll(".course-list");

  archiveCards.forEach((card) => {
    card.style.display = "none";
  });

  currentCards.forEach((card) => {
    card.style.display = "flex";
  });

  btnArchive.style.backgroundColor = "#D9D9D9";
  btnCurrent.style.backgroundColor = "#e9e9e9";
  btnArchive.style.position = "absolute";
  btnArchive.style.left = "9rem";
  btnCurrent.style.position = "relative";

  btnJoin.style.display = "block";
});
