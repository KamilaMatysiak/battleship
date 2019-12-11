#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>

#include <winsock.h>

struct Players
{
	int id;
	SOCKET playersocket;
	struct sockaddr_in endpoint;
	int playerType; //0 - player 1, 1 - player 2, 2 - guest
};
void message(char* tab)
{
	for (int i = 0; i < 7; i++)
		tab[i] = '0';
}
int main(int argc, char **argv)
{
	WSADATA wsaData; // je´sli to nie zadzia³a
	//WSAData wsaData; // uÿzyj tego
	if (WSAStartup(MAKEWORD(1, 1), &wsaData) != 0) {
		fprintf(stderr, "WSAStartup failed.\n");
		exit(1);	}
	char recbuf[8];
	char sendbuf[8];
	struct Players players[10];
	int plays = 0;
	int watches = 2;
	int owner = 0, recipient = 0;
	struct sockaddr_in myaddr, remoteaddr;
	int socketAmount;
	SOCKET sdsocket;
	int addrlen;
	int r;
	unsigned int port = 8888;
	fd_set desset;
	if ((sdsocket = socket(AF_INET, SOCK_DGRAM, 0)) < 0)
	{
		printf("socket() nie powiodl sie\n");
		return 1;
	}

	myaddr.sin_family = AF_INET;
	myaddr.sin_port = htons(port);
	myaddr.sin_addr.s_addr = INADDR_ANY;

	if (bind(sdsocket, (struct sockaddr*) &myaddr, sizeof(struct sockaddr_in)) < 0)
	{
		printf("bind() nie powiodl sie\n");
		perror(WSAGetLastError);
		return 1;
	}
	addrlen = sizeof(struct sockaddr_in);


	// turururu
	socketAmount = sdsocket + 1;
	FD_ZERO(&desset);
	FD_SET(sdsocket, &desset);

	printf("Serwer wystartowal\n");
	while (1)
	{
		memset(recbuf, 0, 8);
		r = 0;
		printf("\n\nSerwer oczekuje\n");
		while (r < 7)
			r += recvfrom(sdsocket, recbuf + r, 8 - r, 0, (struct sockaddr*) &remoteaddr, &addrlen);
		recbuf[7] = '\0';
		printf("\n\nBitow odebranych %d. Wiadomosc od %s: %s\n",
			r,
			inet_ntoa(remoteaddr.sin_addr),
			recbuf);
		if (strcmp(recbuf, "0000000") == 0)
		{
			printf("Prosba o id\n");
			if (plays < 2)
			{
				players[plays].id = plays;
				players[plays].playersocket = sdsocket;
				players[plays].endpoint = remoteaddr;
				players[plays].playerType = plays;
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[2] = plays + '0';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &players[plays].endpoint, addrlen);
				printf("Id wyslane %s\n", sendbuf);
				plays++;
			}
			else
			{
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				printf("Limit graczy osiagniety\n");
			}
		}
		else if (strcmp(recbuf, "0010000") == 0)
		{
			if (watches < 10)
			{
				players[watches].id = watches;
				players[watches].playersocket = sdsocket;
				players[watches].endpoint = remoteaddr;
				players[watches].playerType = 2;
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[2] = watches + '0';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &players[watches].endpoint, addrlen);
				printf("Id wyslane %s\n", sendbuf);
				watches++;
			}
			else
			{
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				printf("Limit ogladajacych osiagniety\n");
			}
		}
		else
		{
			if (plays != 2)
			{
				printf("Oczekujemy nastepnego gracza / bledna wiadomosc \n");
				continue;
			}
			if (recbuf[0] == '0')
			{
				owner = recbuf[1] - '0';
				recipient = (owner + 1) % 2;
				printf("Poprawna wiadomosc dostana %s, %d, %d\n", recbuf, owner, recipient);
				if (recbuf[3] == '1')
				{
					printf("Strzal otrzymany\n");
					memset(sendbuf, 0, 8);
					message(sendbuf);
					sendbuf[1] = recipient + '0';
					sendbuf[2] = owner + '0';
					sendbuf[3] = '2';
					sendbuf[5] = recbuf[5];
					sendbuf[6] = recbuf[6];
					printf("Wiadomosc strzal utworzona %s\n", sendbuf);
					r = 0;
					while (r < 7)
					{
						r += sendto(players[recipient].playersocket, sendbuf + r, 8 - r, 0,
							(struct sockaddr*) &players[recipient].endpoint, addrlen);
						printf("Strzal w trakcie %d\n", r);
					}
					printf("Strzal wyslany %d\n", r);
				}
				else if (recbuf[3] == '2')
				{
					printf("Wynik otrzymany\n");
					for (int i = 0; i < watches; i++)
					{
						if (i == owner) continue;
						memset(sendbuf, 0, 8);
						message(sendbuf);
						sendbuf[1] = i + '0';
						sendbuf[2] = owner + '0';
						sendbuf[3] = '1';
						sendbuf[4] = recbuf[4];
						sendbuf[5] = recbuf[5];
						sendbuf[6] = recbuf[6];
						printf("Wiadomosc wynik utworzona %s\n", sendbuf);
						r = 0;
						while (r < 7)
							r += sendto(players[i].playersocket, sendbuf + r, 8 - r, 0,
							(struct sockaddr*) &players[i].endpoint, addrlen);
						printf("Wynik wyslany %d\n", r);
					}
				}
				else
				{
					memset(sendbuf, 0, 8);
					sendbuf[0] = '1';
					sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
					printf("Zly format \n");
				}

			}

			else
			{
				memset(sendbuf, 0, 8);
				sendbuf[0] = '1';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				printf("Zly format \n");
			}
		}
	}
	return 0;

}