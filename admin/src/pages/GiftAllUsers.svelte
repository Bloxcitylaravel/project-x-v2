<script lang="ts">
	import { navigate } from "svelte-routing";
	import Main from "../components/templates/Main.svelte";
	import request from "../lib/request";
	let disabled = false;
	let errorMessage: string | undefined;
	import * as rank from "../stores/rank";
</script>

<svelte:head>
	<title>Gift all users</title>
</svelte:head>

<Main>
	<div class="row">
		<div class="col-12">
			<h1>Gift users</h1>
			{#if errorMessage}
				<p class="err">{errorMessage}</p>
			{/if}
		</div>
		<div class="col-12">
			<label for="gift-id">Gift ID (The id of the gift)</label>
			<input type="text" class="form-control" id="gift-id" {disabled} />
		</div>
		<div class="col-12">
			<label for="asset-id">Asset ID (The gift to give)</label>
			<input type="text" class="form-control" id="asset-id" {disabled} />
		</div>
		<div class="col-12 mt-4">
			<button
				class="btn btn-success"
				{disabled}
				on:click={(e) => {
					e.preventDefault();
					if (disabled) {
						return;
					}
					disabled = true;
					request
						.post("/gift-users", {
							// @ts-ignore
							giftId: document.getElementById("gift-id").value,
							// @ts-ignore
							assetId: document.getElementById("asset-id").value,
							// @ts-ignore
						})
						.then((d) => {
							disabled = false;
						})
						.catch((e) => {
							errorMessage = e.message;
						})
						.finally(() => {
							disabled = false;
						});
				}}>Gift users</button
			>
		</div>
	</div>
</Main>

<style>
	p.err {
		color: red;
	}
</style>
