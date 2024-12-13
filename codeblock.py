import tkinter as tk
from tkinter import messagebox
import random
import os

class CodeBlockGame:
    def __init__(self, root):
        self.root = root
        self.root.title("Code Block Game")

        self.level_data = {}
        self.current_level = 1

        # Main menu screen
        self.start_screen()

    def start_screen(self):
        self.clear_screen()
        title = tk.Label(self.root, text="Code Block Game", font=("Arial", 24))
        title.pack(pady=20)

        start_button = tk.Button(self.root, text="Start Game", font=("Arial", 14), command=self.load_level)
        start_button.pack(pady=10)

    def load_level(self):
        if not os.path.exists("mainstory.levelmanagement"):
            messagebox.showerror("Error", "Level file not found!")
            return

        self.load_level_data()
        if str(self.current_level) not in self.level_data:
            messagebox.showinfo("Info", "No more levels!")
            self.start_screen()
            return

        level_config = self.level_data[str(self.current_level)]
        self.stay_blocks = set(level_config.get("StayBlock", ""))
        self.order_presets = level_config.get("OrderPresets", {})
        self.goto_levels = level_config.get("GoToLevel", {})
        self.blocks = list(level_config.get("Block", {}).values())

        self.render_level()

    def load_level_data(self):
        self.level_data.clear()

        with open("mainstory.levelmanagement", "r") as file:
            lines = file.readlines()

        level_key = None
        block_data = {}
        i = 0
        while i < len(lines):
            line = lines[i].strip()
            if line.startswith("{Level="):
                level_key = line.split("=")[1][:-1]
                self.level_data[level_key] = {"Block": {}, "OrderPresets": {}, "GoToLevel": {}, "StayBlock": ""}
                i += 1
            elif line.startswith("[Block]"):
                block_data = self.level_data[level_key]["Block"]
                i += 1
                while i < len(lines) and "=" in lines[i]:
                    key, val = lines[i].strip().split("=")
                    block_data[key] = val
                    i += 1
            elif line.startswith("[StayBlock]"):
                i += 1
                if i < len(lines) and "," in lines[i]:
                    self.level_data[level_key]["StayBlock"] = set(lines[i].strip().split(","))
                    i += 1
            elif line.startswith("[OrderPreset="):
                preset_key = line.split("=")[1][:-1]
                i += 1
                if i < len(lines):
                    self.level_data[level_key]["OrderPresets"][preset_key] = lines[i].strip()
                    i += 1
            elif line.startswith("[GoToLevel]"):
                i += 1
                while i < len(lines) and "=" in lines[i]:
                    key, val = lines[i].strip().split("=")
                    self.level_data[level_key]["GoToLevel"][key] = int(val)
                    i += 1
            else:
                i += 1

    def render_level(self):
        self.clear_screen()
        tk.Label(self.root, text=f"Level {self.current_level}", font=("Arial", 18)).pack(pady=10)

        self.target_frame = tk.Frame(self.root)
        self.target_frame.pack(pady=20)

        self.blocks_frame = tk.Frame(self.root)
        self.blocks_frame.pack()

        self.targets = []
        self.draggable_blocks = []
        self.current_order = [None] * len(self.blocks)

        # 动态生成目标位置
        for idx, _ in enumerate(self.blocks):
            target_label = tk.Label(self.target_frame, text="", width=15, height=2, relief=tk.RAISED, bg="lightgray")
            target_label.grid(row=idx, column=0, pady=5)
            self.targets.append(target_label)

        # 动态生成代码块
        shuffled_blocks = [b for b in self.blocks]
        random.shuffle(shuffled_blocks)

        for block in shuffled_blocks:
            block_label = tk.Label(self.blocks_frame, text=block, width=15, height=2, relief=tk.RAISED, bg="white")
            block_label.bind("<Button-1>", self.on_drag_start)
            block_label.bind("<B1-Motion>", self.on_drag_motion)
            block_label.bind("<ButtonRelease-1>", self.on_drag_release)
            block_label.pack(pady=5)
            self.draggable_blocks.append(block_label)

        # 提交按钮
        submit_button = tk.Button(self.root, text="Submit", command=self.check_submission)
        submit_button.pack(pady=10)

    def on_drag_start(self, event):
        widget = event.widget
        widget.lift()  # 提升控件到最高层级
        self.drag_data = {"x": event.x, "y": event.y, "widget": widget}

    def on_drag_motion(self, event):
        widget = self.drag_data["widget"]
        x, y = widget.winfo_x() + event.x - self.drag_data["x"], widget.winfo_y() + event.y - self.drag_data["y"]
        widget.place(x=x, y=y)

    def on_drag_release(self, event):
        widget = self.drag_data["widget"]
        closest_target = None
        min_distance = float("inf")

        for idx, target in enumerate(self.targets):
            if self.current_order[idx] is None:  # Only check empty targets
                distance = self.calculate_distance(widget, target)
                if distance < min_distance:
                    min_distance = distance
                    closest_target = (target, idx)

        if closest_target and min_distance < 50:  # Snap only if close enough
            target, idx = closest_target
            self.current_order[idx] = widget["text"]
            widget.place(x=target.winfo_x(), y=target.winfo_y())
        else:
            widget.place_forget()  # Reset position if not close enough

    def calculate_distance(self, widget1, widget2):
        x1, y1 = widget1.winfo_x(), widget1.winfo_y()
        x2, y2 = widget2.winfo_x(), widget2.winfo_y()
        return ((x1 - x2) ** 2 + (y1 - y2) ** 2) ** 0.5

    def check_submission(self):
        order_str = ''.join(filter(None, self.current_order))
        for preset, preset_order in self.order_presets.items():
            if order_str == preset_order:
                self.current_level = self.goto_levels[preset]
                self.load_level()
                return
        messagebox.showinfo("Result", "Cannot Run")

    def clear_screen(self):
        for widget in self.root.winfo_children():
            widget.destroy()

if __name__ == "__main__":
    root = tk.Tk()
    root.geometry("800x600")
    game = CodeBlockGame(root)
    root.mainloop()
