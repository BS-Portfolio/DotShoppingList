<script setup lang="ts">
import {onMounted, ref} from 'vue';

const shoppingLists = ref<{
  shoppingListId: string;
  shoppingListName: string;
  listOwner: string;
  itemCount: number;
  collaborators: string[];
}[]>([]);
const error = ref<string | null>(null);
const newListName = ref<string>('');

const fetchShoppingLists = async () => {
  try {
    const userData = localStorage.getItem('userData');
    const userID = userData ? JSON.parse(userData).userID : null;
    const userApiKey = userData ? JSON.parse(userData).apiKey : null;

    if (!userData) {
      throw new Error('User data not found in session storage.');
    }

    const response = await fetch(
        `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/all`,
        {
          method: 'GET',
          headers: {
            accept: 'text/plain',
            'USER-KEY': userApiKey,
            'USER-ID': userID,
          },
        }
    );

    if (!response.ok) {
      throw new Error(`Error: ${response.status} ${response.statusText}`);
    }

    const data = await response.json();
    shoppingLists.value = data.map((list: any) => ({
      shoppingListId: list.shoppingListId,
      shoppingListName: list.shoppingListName,
      listOwner: `${list.listOwner.firstName} ${list.listOwner.lastName}`,
      itemCount: list.items.length,
      collaborators: list.collaborators.map(
          (collaborator: any) => `${collaborator.firstName} ${collaborator.lastName}`
      ),
    }));
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const createNewList = async () => {
  if (!newListName.value.trim()) return;

  try {
    const userData = localStorage.getItem('userData');
    const userID = userData ? JSON.parse(userData).userID : null;
    const userApiKey = userData ? JSON.parse(userData).apiKey : null;

    if (!userData) {
      throw new Error('User data not found in session storage.');
    }

    const response = await fetch(
        `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList`,
        {
          method: 'POST',
          headers: {
            accept: 'text/plain',
            'USER-KEY': userApiKey,
            'USER-ID': userID,
            'Content-Type': 'application/json-patch+json',
          },
          body: JSON.stringify(newListName.value),
        }
    );

    if (!response.ok) {
      throw new Error(`Error: ${response.status} ${response.statusText}`);
    }

    newListName.value = '';
    await fetchShoppingLists();
  } catch (err) {
    error.value = (err as Error).message;
  }
};

const removeList = async (listId: string) => {
  const userData = localStorage.getItem('userData');
  const userID = userData ? JSON.parse(userData).userID : null;
  const userApiKey = userData ? JSON.parse(userData).apiKey : null;

  if (!userData) {
    alert('User data not found in session storage.');
    return;
  }

  const confirmed = confirm('Are you sure you want to delete this list?');
  if (!confirmed) return;

  try {
    const response = await fetch(
        `https://localhost:7191/ShoppingListApi/User/${userID}/ShoppingList/${listId}`,
        {
          method: 'DELETE',
          headers: {
            accept: 'text/plain',
            'USER-KEY': userApiKey,
            'USER-ID': userID,
          },
        }
    );

    if (!response.ok) {
      throw new Error(`Error: ${response.status} ${response.statusText}`);
    }

    await fetchShoppingLists();
  } catch (err) {
    error.value = (err as Error).message;
  }
};

onMounted(fetchShoppingLists);
</script>

<template>
  <div>
    <h2>Shopping Lists</h2>
    <ul v-if="shoppingLists.length">
      <li v-for="(list, index) in shoppingLists" :key="index" class="list-item">
        <div class="list-name">
          {{ list.shoppingListName }}
          <button class="remove-button" @click="removeList(list.shoppingListId)">x</button>
        </div>
        <div class="list-details">
          <span><strong>Owner:</strong> {{ list.listOwner }}</span>
          <span><strong>Item Count:</strong> {{ list.itemCount }}</span>
          <span>
            <strong>Collaborators:</strong>
            <template v-if="list.collaborators.length">
              {{ list.collaborators.join(', ') }}
            </template>
            <template v-else>
              None
            </template>
          </span>
        </div>
      </li>
      <li>
        <input
            v-model="newListName"
            placeholder="Enter new list name"
            @keyup.enter="createNewList"
        />
        <button @click="createNewList">Add List</button>
      </li>
    </ul>
    <p v-else-if="error">{{ error }}</p>
    <p v-else>Loading...</p>
  </div>
</template>

<style scoped>
li.list-item {
  position: relative;
  margin-bottom: 1rem;
  padding: 1rem;
  border: 1px solid var(--color-primary);
  border-radius: 4px;
}

li.list-item:hover {
  background-color: #f5a4b8;
  cursor: pointer;
}

.remove-button {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: none;
  border: none;
  color: red;
  font-size: 1.25rem;
  cursor: pointer;
}

.remove-button:hover {
  color: darkred;
}
</style>

<style scoped>
h2 {
  color: var(--color-primary);
  margin-bottom: 0.5rem;
  text-align: center;
}

ul {
  list-style-type: none;
  padding: 0;
}

li {
  margin-bottom: 1rem;
  padding: 1rem;
  border: 1px solid var(--color-primary);
  border-radius: 4px;
}

li:hover {
  background-color: #f5a4b8;
  cursor: pointer;
}

.list-name {
  font-size: 1.25rem;
  font-weight: bold;
  color: var(--color-primary);
  margin-bottom: 0.5rem;
}

.list-details {
  display: flex;
  flex-direction: row;
  gap: 1rem;
}

.list-details strong {
  color: var(--color-primary);
}

input {
  padding: 0.5rem;
  border: 1px solid var(--color-primary);
  border-radius: 4px;
}

button {
  padding: 0.5rem 1rem;
  background-color: var(--color-primary);
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:hover {
  background-color: var(--color-hover);
}

.remove-button {
  background: none;
  border: none;
  color: var(--color-primary);
  font-size: 1.25rem;
  cursor: pointer;
  margin-left: 0.5rem;
}

</style>
