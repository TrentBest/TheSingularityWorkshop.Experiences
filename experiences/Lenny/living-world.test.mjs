import test from "node:test";
import assert from "node:assert/strict";
import { createCleanupState, reduceCleanupState, getDepositedCount } from "./living-world.mjs";

const send = (state, type, extra = {}) => reduceCleanupState(state, { type, ...extra });

test("counter increments only after deposit completes", () => {
  let s = createCleanupState();
  s = send(s, "SELECT_LITTER", { itemId: "litter-01", operationId: "pickup-1" });
  assert.equal(getDepositedCount(s), 0);
  s = send(s, "PICKUP_COMPLETED", { operationId: "pickup-1" });
  assert.equal(getDepositedCount(s), 0);
  s = send(s, "SELECT_RECEPTACLE", { operationId: "deposit-1" });
  assert.equal(getDepositedCount(s), 0);
  s = send(s, "DEPOSIT_COMPLETED", { operationId: "deposit-1" });
  assert.equal(getDepositedCount(s), 1);
});

test("duplicate and stale completion events cannot double-count", () => {
  let s = createCleanupState();
  s = send(s, "SELECT_LITTER", { itemId: "litter-01", operationId: "pickup-1" });
  s = send(s, "PICKUP_COMPLETED", { operationId: "pickup-1" });
  s = send(s, "SELECT_RECEPTACLE", { operationId: "deposit-1" });
  s = send(s, "DEPOSIT_COMPLETED", { operationId: "deposit-1" });
  const completed = s;
  s = send(s, "DEPOSIT_COMPLETED", { operationId: "deposit-1" });
  assert.equal(s, completed);
  assert.equal(getDepositedCount(s), 1);
});

test("interrupted pickup returns the item to the available pool", () => {
  let s = createCleanupState();
  s = send(s, "SELECT_LITTER", { itemId: "litter-01", operationId: "pickup-1" });
  s = send(s, "INTERRUPT");
  assert.equal(s.phase, "idle");
  assert.equal(s.pendingItemId, null);
  assert.equal(getDepositedCount(s), 0);
});

test("navigation while carrying preserves the item without counting it", () => {
  let s = createCleanupState();
  s = send(s, "SELECT_LITTER", { itemId: "litter-01", operationId: "pickup-1" });
  s = send(s, "PICKUP_COMPLETED", { operationId: "pickup-1" });
  s = send(s, "NAVIGATE_AWAY");
  assert.equal(s.phase, "carrying");
  assert.equal(s.heldItemId, "litter-01");
  assert.equal(getDepositedCount(s), 0);
});

test("interrupted deposit returns to carrying and can be completed once", () => {
  let s = createCleanupState();
  s = send(s, "SELECT_LITTER", { itemId: "litter-01", operationId: "pickup-1" });
  s = send(s, "PICKUP_COMPLETED", { operationId: "pickup-1" });
  s = send(s, "SELECT_RECEPTACLE", { operationId: "deposit-1" });
  s = send(s, "INTERRUPT");
  assert.equal(s.phase, "carrying");
  s = send(s, "SELECT_RECEPTACLE", { operationId: "deposit-2" });
  s = send(s, "DEPOSIT_COMPLETED", { operationId: "deposit-2" });
  assert.equal(getDepositedCount(s), 1);
});
