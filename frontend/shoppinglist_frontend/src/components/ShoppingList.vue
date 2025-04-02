<script setup lang="ts">
import {onMounted, ref} from 'vue'

const newItem = ref('')
const items = ref<string[]>([])

const loadItems = () => {
  const storedItems = sessionStorage.getItem('shopping-list')
  if (storedItems) {
    items.value = JSON.parse(storedItems)
  }
}

const saveItems = () => {
  sessionStorage.setItem('shopping-list', JSON.stringify(items.value))
}

const addItem = () => {
  if (newItem.value.trim()) {
    items.value.push(newItem.value.trim())
    newItem.value = ''
    saveItems()
  }
}

const removeItem = (index: number) => {
  items.value.splice(index, 1)
  saveItems()
}

onMounted(loadItems)
</script>

<template>
  <div>
    <h1>Shopping List</h1>
    <input v-model="newItem" @keyup.enter="addItem" placeholder="Add a new item &#9166"/>
    <ul>
      <li v-for="(item, index) in items" :key="index">
        {{ item }}
        <button @click="removeItem(index)" class="remove-button">&times;</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
div {
  display: flex;
  flex-direction: column;
  align-items: center;
}

h1 {
  font-size: 2em;
  margin: 0.5em 0;
  text-align: center;
}

input {
  margin: 0.5em 0;
  width: 90%;
  height: 5vh;
  font-size: 25px;
  text-align: center;
  border: 2px solid var(--primary-color);
  background: transparent;
  color: var(--color-text);
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
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: var(--primary-color);
}

li:nth-child(even) {
  background-color: var(--color-background-soft);
}

.remove-button {
  color: var(--primary-color);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 1.5em;
}
</style>
