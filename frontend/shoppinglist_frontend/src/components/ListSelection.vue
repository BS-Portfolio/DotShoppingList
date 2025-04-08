<script setup lang="ts">
import {onMounted, ref} from 'vue';

const shoppingLists = ref<{
  shoppingListName: string;
  listOwner: string;
  itemCount: number;
  collaborators: string[];
}[]>([]);
const error = ref<string | null>(null);

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

onMounted(fetchShoppingLists);
</script>

<template>
  <div>
    <h2>Lists</h2>
    <ul v-if="shoppingLists.length">
      <li v-for="(list, index) in shoppingLists" :key="index">
        <div class="list-name">{{ list.shoppingListName }}</div>
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
    </ul>
    <p v-else-if="error">{{ error }}</p>
    <p v-else>Loading...</p>
  </div>
</template>

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
</style>
