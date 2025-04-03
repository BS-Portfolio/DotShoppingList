<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();

const newItem = ref('');
const items = ref<string[]>([]);
const quantities = ref<string[]>([]);
const isEditing = ref<number | null>(null);

const loadFromSession = (key: string) => JSON.parse(sessionStorage.getItem(key) || '[]');
const saveToSession = (key: string, value: any) => sessionStorage.setItem(key, JSON.stringify(value));

const loadItems = () => {
  items.value = loadFromSession('shopping-list');
  quantities.value = loadFromSession('shopping-list-quantities');
};

const saveItems = () => {
  saveToSession('shopping-list', items.value);
  saveToSession('shopping-list-quantities', quantities.value);
};

const addItem = () => {
  if (!newItem.value.trim()) return;
  const [item, quantity] = newItem.value.split(',').map(part => part.trim());
  items.value.push(item);
  quantities.value.push(quantity || '1');
  newItem.value = '';
  saveItems();
};

const removeItem = (index: number) => {
  items.value.splice(index, 1);
  quantities.value.splice(index, 1);
  saveItems();
};

const updateQuantity = (index: number, event: KeyboardEvent) => {
  if (event.key === 'Enter') {
    const target = event.target as HTMLInputElement;
    quantities.value[index] = target.value.trim() || '1';
    isEditing.value = null;
    saveItems();
  }
};

const enableEditing = (index: number) => {
  isEditing.value = index;
};

const logout = () => {
  authStore.logout();
  localStorage.removeItem('isAuthenticated');
  router.push('/login');
};

onMounted(loadItems);
</script>

<template>
  <div>
    <h1 class="caveat-brush-regular">Shopping List</h1>
    <input v-model="newItem" @keyup.enter="addItem" placeholder="Add a new item &#9166"/>
    <ul>
      <li v-for="(item, index) in items" :key="index">
        <span class="item-name">{{ item }}</span>
        <input v-if="isEditing === index" type="text" v-model="quantities[index]" @keyup.enter="updateQuantity(index, $event)" class="quantity-input" />
        <span v-else @click="enableEditing(index)">{{ quantities[index] }}</span>
        <button @click="removeItem(index)" class="remove-button">&times;</button>
      </li>
    </ul>
    <button @click="logout" class="logout-button">Logout</button>
  </div>
</template>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Caveat+Brush&display=swap');

div {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

.caveat-brush-regular {
  font-family: "Caveat Brush", cursive;
  font-weight: 400;
  font-style: normal;
}

h1 {
  font-size: 3em;
  text-align: center;
  color: var(--color-primary);
  margin: 0 0 1rem;
}

input {
  width: 90%;
  height: 5vh;
  font-size: 25px;
  text-align: center;
  border: 2px solid var(--color-primary);
  background: transparent;
  color: var(--color-primary);
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
  color: var(--primary-color);
}

.item-name {
  width: 60%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

li:nth-child(even) {
  background-color: #fff8dc;
}

.quantity-input {
  width: 35%;
  margin-left: 1em;
}

.remove-button {
  color: var(--primary-color);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}

.logout-button {
  position: absolute;
  top: 1%;
  right: 10px;
  transform: translateY(-50%);
  padding: 10px 20px;
}
</style>
