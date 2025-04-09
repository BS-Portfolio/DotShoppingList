const btn = document.querySelector(".btn");
const url = "https://localhost:7191/ShoppingListApi/User/Login";

btn.addEventListener("click", function (e) {
  login({
    userName: "bWlsYWRAZGcuY29t",
    password: "NThUczMyJkFCQzRzbEsk",
    url: url,
  });
});

async function login({ userName, password, url }) {
  try {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        Accept: "text/plain",
        "Content-Type": "application/json",
        "X-Frontend": "",
      },
      body: JSON.stringify({
        emailAddress: userName,
        password: password,
      }),
    });

    alert(response.json());
  } catch (e) {
    alert(e.message);
  }
}
