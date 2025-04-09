<script setup lang="ts">
import {onMounted, ref} from 'vue';

const shoppingList = ref<{
  shoppingListName: string;
  items: { itemID: string; name: string; quantity: string }[];
}>({
  shoppingListName: '',
  items: [],
});

const error = ref<string | null>(null);
const successMessage = ref<string | null>(null);
const newItemName = ref<string>('');
const newItemAmount = ref<string>('');
const listId = '442f79cd-3724-4938-9ad2-4f5920b8bab6';

const getUserData = () => {
  const userData = localStorage.getItem('userData');
  if (!userData) throw new Error('User data not found in session storage.');
  return JSON.parse(userData);
};

const fetchWithAuth = async (url: string, options: RequestInit = {}) => {
  const {userID, apiKey} = getUserData();

  const mergedOptions: RequestInit = {
    ...options,
    headers: {
      accept: 'application/json',
      'USER-KEY': apiKey,
      'USER-ID': userID,
      ...options.headers,
    },
  };

  const response = await fetch(url, mergedOptions);

  if (!response.ok) {
    throw new Error(`Error: ${response.status} ${response.statusText}`);
  }

  return response;
};

const handleSuccess = async (message: string) => {
  successMessage.value = message;
  setTimeout(() => (successMessage.value = null), 3000);
  await loadShoppingList(listId);
};

const loadShoppingList = async (listId: string) => {
  try {
    const {userID} = getUserData();
    const response = await fetchWithAuth(
      `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/${listId}`
    );
    const data = await response.json();
    shoppingList.value = {
      shoppingListName: data.shoppingListName,
      items: data.items.map((item: { itemID: string; itemName: string; itemAmount: string }) => ({
        itemID: item.itemID,
        name: item.itemName,
        quantity: item.itemAmount,
      })),
    };
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const addItem = async (name: string, quantity: string) => {
  if (!name.trim() || !quantity.trim()) return;

  try {
    const {userID} = getUserData();
    await fetchWithAuth(
      `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/${listId}/Item`,
      {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({itemName: name, itemAmount: quantity}),
      }
    );
    newItemName.value = '';
    newItemAmount.value = '';
    await handleSuccess('Item added successfully!');
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const updateItem = async (itemID: string, name: string, quantity: string) => {
  if (!name.trim() || !quantity.trim()) return;

  try {
    const {userID} = getUserData();
    await fetchWithAuth(
      `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/${listId}/Item/${itemID}`,
      {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json-patch+json',
          accept: 'text/plain',
        },
        body: JSON.stringify({newItemName: name, newItemAmount: quantity}),
      }
    );
    await handleSuccess('Item updated successfully!');
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const deleteItem = async (itemID: string) => {
  try {
    const {userID} = getUserData();
    await fetchWithAuth(
      `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/${listId}/Item/${itemID}`,
      {
        method: 'DELETE',
        headers: {accept: 'text/plain'},
      }
    );
    await handleSuccess('Item deleted successfully!');
  } catch (err) {
    error.value = (err as Error).message;
  }
};

onMounted(() => {
  void loadShoppingList(listId);
});
</script>


<template>
  <div>
    <h1 class="caveat-brush-regular">Edit Shopping List</h1>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-else>
      <input
        v-model="shoppingList.shoppingListName"
        placeholder="Shopping List Name"
        class="list-name-input"
      />
      <ul>
        <li v-for="(item, index) in shoppingList.items" :key="index" class="list-item">
          <form @submit.prevent="updateItem(item.itemID, item.name, item.quantity)"
                class="item-form">
            <input v-model="item.name" placeholder="Item Name" class="item-input"/>
            <input v-model="item.quantity" placeholder="Quantity" class="quantity-input"/>
            <button type="submit">Update</button>
          </form>
          <button class="remove-button" @click="deleteItem(item.itemID)">x</button>
        </li>
      </ul>
      <form @submit.prevent="addItem(newItemName, newItemAmount)">
        <input v-model="newItemName" placeholder="Item Name"/>
        <input v-model="newItemAmount" placeholder="Item Amount"/>
        <button type="submit">Add Item</button>
      </form>
      <div v-if="successMessage" class="success">{{ successMessage }}</div>
    </div>
  </div>
</template>

<style scoped>
div {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

input {
  width: 90%;
  height: 5vh;
  font-size: 25px;
}

button {
  margin-left: 0.5em;
}

ul {
  list-style-type: none;
  padding: 0;
  width: 90%;
}

li {
  margin: 0.5em 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--color-primary);
}

li:nth-child(even) {
  background-color: #fff8dc;
}

.item-input, .quantity-input {
  width: 60%;
  height: auto;
  font-size: inherit;
  margin: 0;
}

.remove-button {
  color: var(--color-primary);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}
</style>
