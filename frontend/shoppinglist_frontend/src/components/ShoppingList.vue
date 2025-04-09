<script setup lang="ts">
import {onMounted, ref} from 'vue';
import {RouterLink, useRoute} from 'vue-router';

const baseUrl = import.meta.env.VITE_API_BASE_URL;

const shoppingList = ref<{
  shoppingListName: string;
  items: { itemID: string; name: string; quantity: string }[];
}>({
  shoppingListName: '',
  items: [],
});

const error = ref<string | null>(null);
const successMessage = ref<string | null>(null);
const newItemInput = ref<string>('');
const route = useRoute();
const listId = route.query.listId as string;

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
        `${baseUrl}/User/${userID}/ShoppingList/${listId}`);
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

const addItem = async (input: string) => {
  const [name, quantityRaw] = input.split(',').map((str) => str.trim());
  const quantity = quantityRaw || '1';

  if (!name) {
    error.value = 'Invalid input. Use the format "ItemName, Amount".';
    return;
  }

  try {
    const {userID} = getUserData();
    await fetchWithAuth(
        `${baseUrl}/User/${userID}/ShoppingList/${listId}/Item`,
        {
          method: 'POST',
          headers: {'Content-Type': 'application/json'},
          body: JSON.stringify({itemName: name, itemAmount: quantity}),
        }
    );
    newItemInput.value = '';
    await handleSuccess('Item added successfully!');
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const updateItem = async (itemID: string, name: string, quantity: string) => {
  if (!name.trim()) return;

  if (!quantity.trim() || quantity === '0') {
    await deleteItem(itemID);
    return;
  }

  try {
    const {userID} = getUserData();
    await fetchWithAuth(
        `${baseUrl}/User/${userID}/ShoppingList/${listId}/Item/${itemID}`,
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
        `${baseUrl}/User/${userID}/ShoppingList/${listId}/Item/${itemID}`,
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
  if (listId) {
    void loadShoppingList(listId);
  } else {
    error.value = 'No list selected.';
  }
});
</script>

<template>
  <div>
    <h1 class="caveat-brush-regular">Shopping List</h1>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-else>
      <h2 class="list-name-header">{{ shoppingList.shoppingListName }}</h2>
      <input
          v-model="newItemInput"
          placeholder="Add a new item (e.g., 'Apples, 2 kg') ↵"
          @keyup.enter="addItem(newItemInput)"
          class="new-item-input"
      />
      <ul>
        <li v-for="(item, index) in shoppingList.items" :key="index" class="list-item">
          <input
              v-model="item.name"
              class="editable-field"
              @keyup.enter="
        updateItem(item.itemID, item.name, item.quantity);
        ($event.target as HTMLInputElement).blur();
      "
          />

          <input
              v-model="item.quantity"
              class="editable-field"
              @keyup.enter="
        updateItem(item.itemID, item.name, item.quantity);
        ($event.target as HTMLInputElement).blur();
      "
          />

          <button class="remove-button" @click="deleteItem(item.itemID)">×</button>
        </li>
      </ul>
      <div v-if="successMessage" class="success">{{ successMessage }}</div>
      <RouterLink to="/dashboard" class="back-button">Dashboard</RouterLink>
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

.new-item-input {
  width: 230%;
  height: 5vh;
  font-size: 25px;
}

.back-button {
  cursor: pointer;
  background-color: var(--color-primary);
  border: none;
  color: white;
  border-radius: 5px;
  margin-left: 1rem;
  padding: 5px 10px;
  font-size: 16px;
}

.back-button:hover {
  background-color: var(--color-hover);
}


ul {
  list-style-type: none;
  padding: 0;
  width: 230%;
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

.editable-field {
  border: none;
  background: transparent;
  font-size: 1rem;
  color: var(--color-primary);
  padding: 4px 8px;
  width: 40%;
  cursor: text;
}

.editable-field:focus {
  background: transparent;
  border: 1px solid var(--color-primary);
  border-radius: 4px;
  outline: none;
}

.remove-button {
  color: var(--color-primary);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}

h1.caveat-brush-regular,
h2.list-name-header {
  color: var(--color-primary);
}

.error {
  color: red;
  margin-top: 1rem;
}

.success {
  color: green;
  margin-top: 1rem;
}
</style>
