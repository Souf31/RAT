#include <arpa/inet.h>
#include <netinet/in.h>
#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>

#define PORT 8888
#define BUFFER_SIZE 1024
#define MAX_CLIENTS 100

typedef struct {
  char ip[INET_ADDRSTRLEN];
  int used;
} ip_entry;

ip_entry ip_table[MAX_CLIENTS];
pthread_mutex_t ip_table_mutex = PTHREAD_MUTEX_INITIALIZER;

int create_server_socket() {
  int server_fd;
  struct sockaddr_in address;
  int opt = 1;
  int addrlen = sizeof(address);

  if ((server_fd = socket(AF_INET, SOCK_STREAM, 0)) == 0) {
    perror("socket failed");
    exit(EXIT_FAILURE);
  }

  if (setsockopt(server_fd, SOL_SOCKET, SO_REUSEADDR | SO_REUSEPORT, &opt, sizeof(opt))) {
    perror("setsockopt");
    exit(EXIT_FAILURE);
  }

  address.sin_family = AF_INET;
  address.sin_addr.s_addr = INADDR_ANY;
  address.sin_port = htons(PORT);

  if (bind(server_fd, (struct sockaddr *)&address, sizeof(address)) < 0) {
    perror("bind failed");
    exit(EXIT_FAILURE);
  }

  if (listen(server_fd, 3) < 0) {
    perror("listen");
    exit(EXIT_FAILURE);
  }

  return server_fd;
}

char *first_message(int sock, const char *message) {
  send(sock, message, strlen(message), 0);
  char *first_mess = malloc(17);
  int bytes_received = recv(sock, first_mess, 17, 0);
  first_mess[bytes_received] = '\0';
  const char *ack_message = "ACK";
  send(sock, ack_message, strlen(ack_message), 0);
  return first_mess;
}

void first_message_send_data(int sock, const char *message, const char *message2) {
  send(sock, message, strlen(message), 0);

  size_t message2_len = strlen(message2);
  size_t total_sent = 0;

  while (total_sent < message2_len) {
    size_t bytes_to_send = message2_len - total_sent;
    if (bytes_to_send > BUFFER_SIZE) {
      bytes_to_send = BUFFER_SIZE;
    }
    size_t bytes_sent = send(sock, message2 + total_sent, bytes_to_send, 0);
    total_sent += bytes_sent;
  }
}

char *second_message_send_data(int sock) {
  char *first_mess = malloc(17);
  int bytes_received = recv(sock, first_mess, 17, 0);
  first_mess[bytes_received] = '\0';
  const char *ack_message = "ACK";
  send(sock, ack_message, strlen(ack_message), 0);
  return first_mess;
}

char *second_message(int sock, size_t data_size) {
  char *second_mess = malloc(data_size+1);
  size_t bytes_received;
  size_t total_received = 0;

  if (data_size < BUFFER_SIZE) {
    int bytes_received =  recv(sock, second_mess, data_size, 0);
    second_mess[bytes_received] = '\0';
  } else {
    while (total_received < data_size) {
      bytes_received = recv(sock, second_mess + total_received, data_size - total_received, 0);
      if (bytes_received <= 0) {
        break;
      }
      total_received += bytes_received;
    }
    second_mess[total_received] = '\0';
  }
  close(sock);
  return second_mess;
}

int create_socket(const char *ip) {

    int sock = socket(AF_INET, SOCK_STREAM, 0);
    if (sock < 0) {
        perror("socket");
        return -1;
    }

    struct sockaddr_in serv_addr;
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_port = htons(PORT);

    if (inet_pton(AF_INET, ip, &serv_addr.sin_addr) <= 0) {
        perror("inet_pton");
        close(sock);
        return -1;
    }

    if (connect(sock, (struct sockaddr *)&serv_addr, sizeof(serv_addr)) < 0) {
        perror("connect");
        close(sock);
        return -1;
    }

    return sock;
}

void store_ip(const char *ip) {
  pthread_mutex_lock(&ip_table_mutex);

  for (int i = 0; i < MAX_CLIENTS; i++) {
    if (!ip_table[i].used) {
      strncpy(ip_table[i].ip, ip, INET_ADDRSTRLEN);
      ip_table[i].used = 1;
      break;
    }
  }

  pthread_mutex_unlock(&ip_table_mutex);
}

void accept_connections(int server_fd) {
  struct sockaddr_in address;
  int addrlen = sizeof(address);

  while (1) {
    int client_fd = accept(server_fd, (struct sockaddr *)&address, (socklen_t *)&addrlen);
    if (client_fd < 0) {
      perror("accept");
      exit(EXIT_FAILURE);
    }

    char ip[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &(address.sin_addr), ip, INET_ADDRSTRLEN);
    store_ip(ip);

    close(client_fd);
  }
}

char *get_stored_ips() {
  pthread_mutex_lock(&ip_table_mutex);

  int buffer_size = MAX_CLIENTS * INET_ADDRSTRLEN;
  char *buffer = malloc(buffer_size);
  memset(buffer, 0, buffer_size);

  for (int i = 0; i < MAX_CLIENTS; i++) {
    if (ip_table[i].used) {
      strncat(buffer, ip_table[i].ip, INET_ADDRSTRLEN - 1);
      strncat(buffer, ",", 1);
    }
  }

  pthread_mutex_unlock(&ip_table_mutex);

  return buffer;
}

